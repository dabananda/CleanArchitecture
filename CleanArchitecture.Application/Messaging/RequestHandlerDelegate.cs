namespace CleanArchitecture.Application.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
