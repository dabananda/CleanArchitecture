using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Role.GetAll;

public class GetAllRolesQueryHandler(
    IRoleRepository roleRepository,
    ICacheService cacheService) : IRequestHandler<GetAllRolesQuery, Result<IEnumerable<RoleResponse>>>
{
    private const string CacheKey = "roles:all";

    public async Task<Result<IEnumerable<RoleResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<List<RoleResponse>>(CacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<IEnumerable<RoleResponse>>.Success(cached);
        }

        var roles = await roleRepository.GetAllAsync(cancellationToken);
        var response = roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description)).ToList();

        await cacheService.SetAsync(CacheKey, response, TimeSpan.FromMinutes(30), cancellationToken);

        return Result<IEnumerable<RoleResponse>>.Success(response);
    }
}
