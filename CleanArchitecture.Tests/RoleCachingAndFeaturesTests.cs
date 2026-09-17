using CleanArchitecture.Application.Behaviors;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Features.Role;
using CleanArchitecture.Application.Features.Role.Create;
using CleanArchitecture.Application.Features.Role.GetAll;
using CleanArchitecture.Application.Features.Role.GetById;
using CleanArchitecture.Application.Features.Role.Update;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;
using CleanArchitecture.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;
using Xunit;

namespace CleanArchitecture.Tests;

public class RoleCachingAndFeaturesTests
{
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    [Fact]
    public async Task GetAllRoles_WhenCacheHit_ShouldReturnCachedListWithoutCallingRepo()
    {
        var cachedRoles = new List<RoleResponse>
        {
            new(Guid.NewGuid(), "Admin", "Admin role"),
            new(Guid.NewGuid(), "User", "User role")
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<RoleResponse>>("roles:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedRoles);

        var handler = new GetAllRolesQueryHandler(_roleRepoMock.Object, _cacheServiceMock.Object);

        var result = await handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(cachedRoles);
        _roleRepoMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllRoles_WhenCacheMiss_ShouldQueryRepoAndPopulateCache()
    {
        _cacheServiceMock
            .Setup(x => x.GetAsync<List<RoleResponse>>("roles:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<RoleResponse>?)null);

        var rolesFromDb = new List<Role> { Role.Create("Admin", "Admin role") };
        _roleRepoMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesFromDb);

        var handler = new GetAllRolesQueryHandler(_roleRepoMock.Object, _cacheServiceMock.Object);

        var result = await handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        _roleRepoMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("roles:all", It.IsAny<List<RoleResponse>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoleById_WhenCacheHit_ShouldReturnCachedWithoutCallingRepo()
    {
        var roleId = Guid.NewGuid();
        var cachedRole = new RoleResponse(roleId, "Manager", "Manager role");

        _cacheServiceMock
            .Setup(x => x.GetAsync<RoleResponse>($"roles:id:{roleId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedRole);

        var handler = new GetRoleByIdQueryHandler(_roleRepoMock.Object, _cacheServiceMock.Object);

        var result = await handler.Handle(new GetRoleByIdQuery(roleId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(cachedRole);
        _roleRepoMock.Verify(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateRole_ShouldInvalidateRoleCache()
    {
        _roleRepoMock.Setup(x => x.ExistsByNameAsync("Support", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateRoleCommandHandler(_roleRepoMock.Object, _cacheServiceMock.Object);

        var result = await handler.Handle(new CreateRoleCommand("Support", "Support role"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _roleRepoMock.Verify(x => x.AddRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("roles:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_ShouldInvalidateAllAndIndividualRoleCache()
    {
        var roleId = Guid.NewGuid();
        var role = Role.Create("OldName", "OldDesc");

        _roleRepoMock.Setup(x => x.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.ExistsByNameAsync("NewName", roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateRoleCommandHandler(_roleRepoMock.Object, _cacheServiceMock.Object);

        var result = await handler.Handle(new UpdateRoleCommand(roleId, "NewName", "NewDesc"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _roleRepoMock.Verify(x => x.UpdateRoleAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("roles:all", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"roles:id:{roleId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidationBehavior_WhenNoValidators_ShouldExecuteNextDirectly()
    {
        var behavior = new ValidationBehavior<CreateRoleCommand, Result>([]);
        var nextExecuted = false;

        var result = await behavior.Handle(
            new CreateRoleCommand("Admin", null),
            CancellationToken.None,
            () =>
            {
                nextExecuted = true;
                return Task.FromResult(Result.Success());
            });

        result.IsSuccess.Should().BeTrue();
        nextExecuted.Should().BeTrue();
    }
}
