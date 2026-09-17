using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IAuthRepository authRepository,
    ICurrentUserService currentUserService) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var ip = currentUserService.IpAddress;
        var user = await authRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null)
            return Result.Success();

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (token is null || token.IsRevoked)
            return Result.Success();

        user.RevokeRefreshToken(request.RefreshToken, ip);

        await authRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
