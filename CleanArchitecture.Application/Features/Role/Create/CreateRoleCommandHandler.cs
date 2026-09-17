using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;
using RoleEntity = CleanArchitecture.Domain.Entities.Role;

namespace CleanArchitecture.Application.Features.Role.Create;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    ICacheService cacheService) : IRequestHandler<CreateRoleCommand, Result>
{
    public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure(ErrorType.Conflict, RoleErrors.AlreadyExists);

        var role = RoleEntity.Create(request.Name, request.Description);

        await roleRepository.AddRoleAsync(role, cancellationToken);
        await cacheService.RemoveAsync("roles:all", cancellationToken);

        return Result.Success();
    }
}
