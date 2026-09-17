using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidPasswordResetToken);

        var token = user.PasswordResetTokens.First(t => t.Token == request.Token);

        if (token.IsConsumed)
            return Result.Failure(ErrorType.Unauthorized, AuthErrors.InvalidPasswordResetToken);

        if (token.IsExpired)
            return Result.Failure(ErrorType.Unauthorized, AuthErrors.PasswordResetTokenExpired);

        var newPasswordHash = passwordHasher.HashPassword(request.NewPassword);

        user.ResetPassword(request.Token, newPasswordHash);

        await authRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
