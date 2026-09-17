using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Delete;

public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
