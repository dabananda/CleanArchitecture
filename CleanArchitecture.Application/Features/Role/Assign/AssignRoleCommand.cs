using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Assign;

public record AssignRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
