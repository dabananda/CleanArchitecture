using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Features.Auth.Login;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;
