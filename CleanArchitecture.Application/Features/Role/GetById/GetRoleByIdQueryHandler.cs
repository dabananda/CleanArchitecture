using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.GetById;

public class GetRoleByIdQueryHandler(
    IRoleRepository roleRepository,
    ICacheService cacheService) : IRequestHandler<GetRoleByIdQuery, Result<RoleResponse>>
{
    public async Task<Result<RoleResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"roles:id:{request.Id}";
        var cached = await cacheService.GetAsync<RoleResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<RoleResponse>.Success(cached);
        }

        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (role is null)
            return Result<RoleResponse>.Failure(ErrorType.NotFound, RoleErrors.NotFound);

        var response = new RoleResponse(role.Id, role.Name, role.Description);

        await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30), cancellationToken);

        return Result<RoleResponse>.Success(response);
    }
}
