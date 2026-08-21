using Academy.Application.Common.Models;
using MediatR;

namespace Academy.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(int UserId) : IRequest<Result>;
