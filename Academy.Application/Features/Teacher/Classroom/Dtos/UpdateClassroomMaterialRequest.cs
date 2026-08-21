using Academy.Domain.Enums;

namespace Academy.Application.Features.Teacher.Classroom.Dtos;

public sealed class UpdateClassroomMaterialRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public ClassroomMaterialType? MaterialType { get; set; }

    public string? ExternalUrl { get; set; }

    public string? Body { get; set; }

    public int? SortOrder { get; set; }
}
