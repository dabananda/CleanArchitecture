using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;

namespace CleanArchitecture.Application.Features.Auth.Register;

public record RegisterCommand(
    string Email,
    string Password) : IRequest<Result>;
