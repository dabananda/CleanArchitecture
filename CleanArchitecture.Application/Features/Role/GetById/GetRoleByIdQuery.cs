using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.GetById;

public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleResponse>>;
