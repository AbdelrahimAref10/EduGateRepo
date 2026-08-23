using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.SuperAdmin.Groups.Queries.GetAdminStudentExamReview;

public sealed record GetAdminStudentExamReviewQuery(int SessionId, int StudentId)
    : IRequest<Result<TeacherStudentExamReviewDto>>;
