using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Remove;

public record RemoveRoleCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
