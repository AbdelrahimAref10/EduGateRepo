using TeacherEntity = Academy.Domain.Entities.Teacher;

namespace Academy.Application.Features.Marketplace;

public readonly record struct TeacherRatingSnapshot(decimal Average, int Count, int Stars)
{
    public static TeacherRatingSnapshot Empty { get; } = new(0, 0, 0);
}

public static class TeacherRatingCalculator
{
    public static TeacherRatingSnapshot From(IEnumerable<int> ratings)
    {
        var values = ratings as IReadOnlyCollection<int> ?? ratings.ToList();
        if (values.Count == 0)
            return TeacherRatingSnapshot.Empty;

        var average = Math.Round(
            (decimal)values.Average(),
            2,
            MidpointRounding.AwayFromZero);

        return new TeacherRatingSnapshot(average, values.Count, FilledStars(average, values.Count));
    }

    public static void Apply(TeacherEntity teacher, IEnumerable<int> ratings)
    {
        var snapshot = From(ratings);
        teacher.RatingCount = snapshot.Count;
        teacher.RatingAverage = snapshot.Average;
    }

    public static int FilledStars(decimal average, int count)
    {
        if (count <= 0)
            return 0;

        return Math.Clamp((int)Math.Round(average, MidpointRounding.AwayFromZero), 1, 5);
    }
}
