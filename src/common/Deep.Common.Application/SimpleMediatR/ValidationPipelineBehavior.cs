using Deep.Common.Domain;
using FluentValidation;
using FluentValidation.Results;

namespace Deep.Common.Application.SimpleMediatR;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IRequestHandler<TRequest, TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _inner;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(
        IRequestHandler<TRequest, TResponse> inner,
        IEnumerable<IValidator<TRequest>> validators
    )
    {
        _inner = inner;
        _validators = validators;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!_validators.Any())
        {
            return await _inner.Handle(request, cancellationToken);
        }

        ValidationFailure[] failures = await ValidateAsync(request, cancellationToken);

        if (failures.Length == 0)
        {
            return await _inner.Handle(request, cancellationToken);
        }

        return Result<TResponse>.ValidationFailure(CreateValidationError(failures));
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))
        );

        return results.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToArray();
    }

    private static ValidationError CreateValidationError(IEnumerable<ValidationFailure> failures) =>
        new(failures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
