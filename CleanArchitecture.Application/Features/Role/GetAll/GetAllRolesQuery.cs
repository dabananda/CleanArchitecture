using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.GetAll;

public record GetAllRolesQuery() : IRequest<Result<IEnumerable<RoleResponse>>>;
