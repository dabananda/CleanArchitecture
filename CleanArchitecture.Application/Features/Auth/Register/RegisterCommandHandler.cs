using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Features.Auth.Register;

public class RegisterCommandHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator secureTokenGenerator,
    IEmailService emailService) : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await authRepository.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Failure(ErrorType.Conflict, AuthErrors.EmailAlreadyExists);

        var role = await roleRepository.GetRoleByNameAsync(Roles.User, cancellationToken);
        if (role is null)
            return Result.Failure(ErrorType.NotFound, RoleErrors.NotFound);

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var user = ApplicationUser.Create(request.Email, passwordHash);

        user.AssignRole(role);

        var verificationToken = secureTokenGenerator.GenerateToken();
        var verificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(60);
        user.IssueEmailVerificationToken(verificationToken, verificationTokenExpiresAt);

        await authRepository.RegisterAsync(user, cancellationToken);

        await emailService.SendEmailVerificationLinkAsync(user.Username, user.Email, verificationToken, cancellationToken);

        return Result.Success("User registered successfully");
    }
}
