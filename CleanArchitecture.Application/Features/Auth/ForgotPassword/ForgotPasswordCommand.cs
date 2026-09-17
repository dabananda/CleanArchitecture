using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
