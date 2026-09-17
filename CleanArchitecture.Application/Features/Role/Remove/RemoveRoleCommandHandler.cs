using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Remove;

public class RemoveRoleCommandHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository) : IRequestHandler<RemoveRoleCommand, Result>
{
    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(ErrorType.NotFound, "User not found.");
        }

        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);
        }

        user.RemoveRole(request.RoleId);
        await authRepository.UpdateAsync(user, cancellationToken);

        return Result.Success("Role removed successfully.");
    }
}
