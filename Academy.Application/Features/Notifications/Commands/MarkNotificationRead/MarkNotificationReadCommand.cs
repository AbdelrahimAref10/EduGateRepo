using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(int UserId, int NotificationId)
    : IRequest<Result>;
