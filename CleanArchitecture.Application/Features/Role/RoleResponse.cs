namespace CleanArchitecture.Application.Features.Role;

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description);
