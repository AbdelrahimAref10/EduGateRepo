using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Academy.Application.Common.Models;
using Academy.Application.Contracts.Ai;
using Academy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Academy.Infrastructure.Ai;

public sealed class GeminiExamGenerator(
    HttpClient httpClient,
    IOptions<AiOptions> options,
    IExamGenerationProgress progress,
    ILogger<GeminiExamGenerator> logger) : IAiExamGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<GeneratedExam>> GenerateAsync(
        GenerateExamAiRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return Result<GeneratedExam>.Failure(
                "مفتاح Gemini غير موجود. ضع Ai:ApiKey في User Secrets أو appsettings.Development.json.");

        if (request.Materials.Count == 0)
            return Result<GeneratedExam>.Failure("لا توجد مواد كافية لتوليد الامتحان.");

        await progress.ReportAsync(request.UserId, ExamGenerationSteps.Generate(), cancellationToken);

        var payload = BuildPayload(request);
        var models = DistinctModels(settings.Model);
        string? lastError = null;

        using var gate = await GeminiFreeTierGate.EnterAsync(cancellationToken);

        foreach (var model in models)
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                var url = $"{settings.BaseUrl.TrimEnd('/')}/models/{model}:generateContent";
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.TryAddWithoutValidation("x-goog-api-key", settings.ApiKey);
                httpRequest.Content = JsonContent.Create(payload);

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(httpRequest, cancellationToken);
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    lastError = "انتهت مهلة الاتصال بـ Gemini. جرّب ملفات أقل أو أصغر.";
                    logger.LogWarning("Gemini exam generate timed out on model {Model}.", model);
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to call Gemini exam generator.");
                    lastError = "تعذر الاتصال بـ Gemini. حاول مرة أخرى.";
                    break;
                }

                using var activeResponse = response;
                var body = await activeResponse.Content.ReadAsStringAsync(cancellationToken);
                if (!activeResponse.IsSuccessStatusCode)
                {
                    var status = (int)activeResponse.StatusCode;
                    lastError = ReadGeminiError(body) ?? $"Gemini HTTP {status}";
                    logger.LogWarning(
                        "Gemini exam generate failed ({Status}) model {Model} attempt {Attempt}: {Body}",
                        status, model, attempt, body);

                    if (IsDailyQuota(status, body, lastError))
                        return Result<GeneratedExam>.Failure(
                            "تم استهلاك حد Gemini اليومي. حاول لاحقاً اليوم أو غداً.");

                    if (IsRetryable(status, body, lastError))
                    {
                        var delay = RetryDelay(body, attempt);
                        GeminiFreeTierGate.Cooldown(delay);
                        if (attempt < 2)
                        {
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        break;
                    }

                    if (status is 404 or 400)
                        break;

                    return Result<GeneratedExam>.Failure(MapUserError(lastError));
                }

                GeminiResponse? parsed;
                try
                {
                    parsed = JsonSerializer.Deserialize<GeminiResponse>(body, JsonOptions);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse Gemini envelope.");
                    return Result<GeneratedExam>.Failure("استجابة Gemini غير صالحة.");
                }

                var text = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?
                    .Select(p => p.Text)
                    .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

                if (string.IsNullOrWhiteSpace(text))
                    return Result<GeneratedExam>.Failure("Gemini لم يُرجع امتحاناً.");

                GeminiExamDto? examDto;
                try
                {
                    examDto = JsonSerializer.Deserialize<GeminiExamDto>(StripMarkdownFence(text), JsonOptions);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse Gemini exam JSON: {Text}", text);
                    return Result<GeneratedExam>.Failure("تعذر قراءة أسئلة الامتحان المولّدة.");
                }

                return Validate(examDto);
            }
        }

        return Result<GeneratedExam>.Failure(MapUserError(lastError) ?? "فشل توليد الامتحان من Gemini.");
    }

    private static string MapUserError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return "تعذر إنشاء الامتحان حالياً. حاول مرة أخرى.";

        if (error.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase)
            || error.Contains("quota", StringComparison.OrdinalIgnoreCase)
            || error.Contains("high demand", StringComparison.OrdinalIgnoreCase)
            || error.Contains("overloaded", StringComparison.OrdinalIgnoreCase)
            || error.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            return "الخدمة مشغولة الآن. انتظر دقيقة ثم حاول مرة أخرى.";
        }

        if (error.StartsWith("Gemini HTTP", StringComparison.OrdinalIgnoreCase))
            return "تعذر إنشاء الامتحان حالياً. حاول مرة أخرى.";

        return error.Length > 180 ? error[..180] : error;
    }

    private static string[] DistinctModels(string configured)
    {
        var list = new[]
        {
            configured,
            "gemini-3.6-flash",
            "gemini-2.5-flash-lite"
        };

        return list
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Where(m => !m.Equals("gemini-2.0-flash", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToArray();
    }

    private static bool IsRetryable(int statusCode, string body, string? error)
    {
        if (statusCode is 429 or 503 or 529)
            return !IsDailyQuota(statusCode, body, error);

        if (string.IsNullOrWhiteSpace(error) && string.IsNullOrWhiteSpace(body))
            return false;

        var text = $"{error} {body}";
        return text.Contains("high demand", StringComparison.OrdinalIgnoreCase)
            || text.Contains("try again later", StringComparison.OrdinalIgnoreCase)
            || text.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase)
            || text.Contains("overloaded", StringComparison.OrdinalIgnoreCase)
            || text.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDailyQuota(int statusCode, string body, string? error)
    {
        if (statusCode != 429)
            return false;

        var text = $"{error} {body}";
        if (text.Contains("PerMinute", StringComparison.OrdinalIgnoreCase)
            || text.Contains("RequestsPerMinute", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return text.Contains("PerDay", StringComparison.OrdinalIgnoreCase)
            || text.Contains("RequestsPerDay", StringComparison.OrdinalIgnoreCase)
            || text.Contains("per day", StringComparison.OrdinalIgnoreCase);
    }

    private static TimeSpan RetryDelay(string body, int attempt)
    {
        var parsed = ParseRetryDelay(body);
        var fallback = Backoff(attempt, TimeSpan.FromSeconds(2));
        var delay = parsed ?? fallback;
        if (delay < TimeSpan.FromSeconds(2))
            delay = TimeSpan.FromSeconds(2);
        if (delay > TimeSpan.FromSeconds(6))
            delay = TimeSpan.FromSeconds(6);
        return delay;
    }

    private static TimeSpan Backoff(int attempt, TimeSpan first)
    {
        var seconds = first.TotalSeconds * Math.Pow(2, Math.Max(0, attempt - 1));
        return TimeSpan.FromSeconds(Math.Min(12, seconds));
    }

    private static TimeSpan? ParseRetryDelay(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            foreach (var value in WalkStrings(doc.RootElement))
            {
                if (!TryParseDuration(value, out var delay))
                    continue;
                if (delay > TimeSpan.Zero)
                    return delay;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static IEnumerable<string> WalkStrings(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name.Equals("retryDelay", StringComparison.OrdinalIgnoreCase)
                        && property.Value.ValueKind == JsonValueKind.String
                        && property.Value.GetString() is { } delay)
                    {
                        yield return delay;
                    }

                    foreach (var child in WalkStrings(property.Value))
                        yield return child;
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var child in WalkStrings(item))
                        yield return child;
                }

                break;
        }
    }

    private static bool TryParseDuration(string raw, out TimeSpan delay)
    {
        delay = TimeSpan.Zero;
        var value = raw.Trim();
        if (value.Length < 2)
            return false;

        var unit = value[^1];
        if (!double.TryParse(value[..^1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            return false;
        }

        delay = unit switch
        {
            's' or 'S' => TimeSpan.FromSeconds(number),
            'm' or 'M' => TimeSpan.FromMinutes(number),
            'h' or 'H' => TimeSpan.FromHours(number),
            _ => TimeSpan.Zero
        };
        return delay > TimeSpan.Zero;
    }

    private static string? ReadGeminiError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                var text = message.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Length > 300 ? text[..300] : text;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static object BuildPayload(GenerateExamAiRequest request)
    {
        var parts = new List<object>
        {
            new { text = BuildPrompt(request) }
        };

        var attached = 0L;
        const long maxInlineBytes = 6 * 1024 * 1024;
        foreach (var material in request.Materials)
        {
            if (material.FileBytes is not { Length: > 0 } bytes
                || bytes.Length > 4 * 1024 * 1024
                || attached + bytes.Length > maxInlineBytes
                || string.IsNullOrWhiteSpace(material.MimeType)
                || !IsGeminiInlineType(material.MimeType))
            {
                continue;
            }

            attached += bytes.Length;
            parts.Add(new Dictionary<string, object>
            {
                ["inlineData"] = new Dictionary<string, string>
                {
                    ["mimeType"] = material.MimeType,
                    ["data"] = Convert.ToBase64String(bytes)
                }
            });
        }

        return new
        {
            contents = new[]
            {
                new { role = "user", parts }
            },
            generationConfig = new
            {
                temperature = 0.3,
                responseMimeType = "application/json"
            }
        };
    }

    private static string BuildPrompt(GenerateExamAiRequest request)
    {
        var language = request.Language == AppLanguage.English ? "English" : "Arabic";
        var subject = string.IsNullOrWhiteSpace(request.Subject) ? "the session subject" : request.Subject.Trim();
        var topic = string.IsNullOrWhiteSpace(request.Topic) ? "the uploaded classroom materials" : request.Topic.Trim();

        var textBlocks = request.Materials
            .Select((m, i) =>
            {
                var body = string.IsNullOrWhiteSpace(m.Text) ? "(file attached or no extra text)" : m.Text.Trim();
                return $"Material {i + 1}: {m.Title}\n{body}";
            });

        return
            $"""
            You are an exam writer for a tutoring classroom.
            Create exactly {request.QuestionCount} multiple-choice questions from ONLY the files the teacher uploaded.
            Subject: {subject}
            Session topic: {topic}
            Write the exam title and all questions in {language}.

            Strict rules:
            - Use ONLY facts, explanations, formulas, and examples that appear in the uploaded files/images.
            - If an attachment is an image, read the writing/diagrams in the image.
            - Do not invent curriculum, names, numbers, or lessons that are not in the uploads.
            - If the files are not enough for {request.QuestionCount} questions, still stay inside the uploaded content and write the best possible questions from it.
            - Each question must have exactly 4 options.
            - Exactly one option per question must have isCorrect=true.
            - No duplicate questions.
            - Return JSON only with title and questions. Each question has text and 4 options. Each option has text and isCorrect.

            Teacher uploads:
            {string.Join("\n\n", textBlocks)}
            """;
    }

    private static Result<GeneratedExam> Validate(GeminiExamDto? dto)
    {
        if (dto is null || dto.Questions is null || dto.Questions.Count == 0)
            return Result<GeneratedExam>.Failure("الامتحان المولّد لا يحتوي على أسئلة.");

        var title = string.IsNullOrWhiteSpace(dto.Title) ? "امتحان الحصة" : dto.Title.Trim();
        var questions = new List<GeneratedExamQuestion>();

        foreach (var question in dto.Questions)
        {
            if (string.IsNullOrWhiteSpace(question.Text) || question.Options is null)
                continue;

            var options = question.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                .Select(o => new GeneratedExamOption
                {
                    Text = o.Text.Trim(),
                    IsCorrect = o.IsCorrect
                })
                .ToList();

            if (options.Count < 2)
                continue;

            if (options.Count(o => o.IsCorrect) != 1)
            {
                if (options.Count(o => o.IsCorrect) == 0)
                    options[0] = new GeneratedExamOption { Text = options[0].Text, IsCorrect = true };
                else
                    options = options
                        .Select((o, i) => new GeneratedExamOption { Text = o.Text, IsCorrect = i == options.FindIndex(x => x.IsCorrect) })
                        .ToList();
            }

            questions.Add(new GeneratedExamQuestion
            {
                Text = question.Text.Trim(),
                Options = options
            });
        }

        if (questions.Count == 0)
            return Result<GeneratedExam>.Failure("لم يتم توليد أسئلة صالحة من المواد.");

        return Result<GeneratedExam>.Success(new GeneratedExam
        {
            Title = title,
            Questions = questions
        });
    }

    private static bool IsGeminiInlineType(string mimeType) =>
        mimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
        || mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    private static string StripMarkdownFence(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            return trimmed;

        var firstBreak = trimmed.IndexOf('\n');
        if (firstBreak < 0)
            return trimmed;

        trimmed = trimmed[(firstBreak + 1)..];
        if (trimmed.EndsWith("```", StringComparison.Ordinal))
            trimmed = trimmed[..^3];

        return trimmed.Trim();
    }

    private sealed class GeminiExamDto
    {
        public string? Title { get; set; }

        public List<GeminiQuestionDto>? Questions { get; set; }
    }

    private sealed class GeminiQuestionDto
    {
        public string? Text { get; set; }

        public List<GeminiOptionDto>? Options { get; set; }
    }

    private sealed class GeminiOptionDto
    {
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
