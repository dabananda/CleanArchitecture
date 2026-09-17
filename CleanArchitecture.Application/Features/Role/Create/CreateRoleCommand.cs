using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Create;

public record CreateRoleCommand(
    string Name,
    string? Description) : IRequest<Result>;
