using Academy.Application.Common.Models;
using Academy.Application.Features.Marketplace.Dtos;
using MediatR;

namespace Academy.Application.Features.Marketplace.Queries.GetPublicHighlights;

public sealed record GetPublicHighlightsQuery
    : IRequest<Result<PublicHighlightsDto>>;
