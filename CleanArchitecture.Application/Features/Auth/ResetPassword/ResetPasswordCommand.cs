using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result>;
