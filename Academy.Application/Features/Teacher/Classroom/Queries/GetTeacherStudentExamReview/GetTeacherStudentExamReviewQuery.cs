using Academy.Application.Common.Models;
using Academy.Application.Features.Teacher.Classroom.Dtos;
using MediatR;

namespace Academy.Application.Features.Teacher.Classroom.Queries.GetTeacherStudentExamReview;

public sealed record GetTeacherStudentExamReviewQuery(int UserId, int SessionId, int StudentId)
    : IRequest<Result<TeacherStudentExamReviewDto>>;
