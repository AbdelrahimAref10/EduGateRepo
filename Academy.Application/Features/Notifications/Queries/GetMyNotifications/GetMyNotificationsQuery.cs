using Academy.Application.Common.Models;
using Academy.Application.Features.Notifications.Dtos;
using MediatR;

namespace Academy.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery(int UserId)
    : IRequest<Result<IReadOnlyList<NotificationDto>>>;
