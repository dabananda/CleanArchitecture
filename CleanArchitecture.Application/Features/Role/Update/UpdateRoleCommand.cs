using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Update;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description) : IRequest<Result>;
