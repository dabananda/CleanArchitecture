using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Update;

public class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    ICacheService cacheService) : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
            return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);

        if (await roleRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken))
            return Result.Failure(ErrorType.Conflict, RoleErrors.AlreadyExists);

        role.UpdateDetails(request.Name, request.Description);

        await roleRepository.UpdateRoleAsync(role, cancellationToken);
        await cacheService.RemoveAsync("roles:all", cancellationToken);
        await cacheService.RemoveAsync($"roles:id:{request.Id}", cancellationToken);

        return Result.Success();
    }
}
