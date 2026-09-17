using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Messaging;
using FluentValidation;

namespace CleanArchitecture.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IValidator<TRequest>[] _validators = validators as IValidator<TRequest>[] ?? [.. validators];

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Length == 0)
        {
            return await next();
        }

        List<string>? failures = null;

        for (var i = 0; i < _validators.Length; i++)
        {
            var result = await _validators[i].ValidateAsync(request, cancellationToken);

            if (!result.IsValid)
            {
                failures ??= new List<string>(result.Errors.Count);
                for (var j = 0; j < result.Errors.Count; j++)
                {
                    var error = result.Errors[j];
                    failures.Add(string.IsNullOrWhiteSpace(error.PropertyName)
                        ? error.ErrorMessage
                        : $"{error.PropertyName}: {error.ErrorMessage}");
                }
            }
        }

        if (failures is null || failures.Count == 0)
        {
            return await next();
        }

        return (TResponse)TResponse.Failure(ErrorType.Validation, "One or more validation errors occurred.", failures);
    }
}
