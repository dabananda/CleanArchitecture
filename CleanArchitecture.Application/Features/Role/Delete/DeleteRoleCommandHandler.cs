using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Delete;

public class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    ICacheService cacheService) : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
            return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);

        role.MarkDelete();

        await roleRepository.UpdateRoleAsync(role, cancellationToken);
        await cacheService.RemoveAsync("roles:all", cancellationToken);
        await cacheService.RemoveAsync($"roles:id:{request.Id}", cancellationToken);

        return Result.Success();
    }
}
