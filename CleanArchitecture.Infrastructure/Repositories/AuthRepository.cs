using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories;

public class AuthRepository(ApplicationDbContext context) : IAuthRepository
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task RegisterAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.RefreshTokens.Where(rt => rt.Token == refreshToken))
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddEmailVerificationTokenAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        if (context.Entry(token).State == EntityState.Detached)
        {
            await context.EmailVerificationTokens.AddAsync(token, cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailVerificationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.EmailVerificationTokens.Where(t => t.Token == token))
            .FirstOrDefaultAsync(u => u.EmailVerificationTokens.Any(t => t.Token == token), cancellationToken);
    }

    public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await context.PasswordResetTokens.AddAsync(token, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(u => u.PasswordResetTokens.Where(t => t.Token == token))
            .Include(u => u.RefreshTokens.Where(rt => rt.RevokedAt == null && rt.ExpireAt > DateTime.UtcNow))
            .FirstOrDefaultAsync(u => u.PasswordResetTokens.Any(t => t.Token == token), cancellationToken);
    }
}
