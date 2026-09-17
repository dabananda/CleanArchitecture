using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.Assign;

public class AssignRoleCommandHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository) : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
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

        user.AssignRole(role);
        await authRepository.UpdateAsync(user, cancellationToken);

        return Result.Success("Role assigned successfully.");
    }
}
