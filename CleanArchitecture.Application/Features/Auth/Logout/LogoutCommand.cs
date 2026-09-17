using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
