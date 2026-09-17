using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>;
