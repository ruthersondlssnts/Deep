using System.ComponentModel.DataAnnotations;
using Deep.Common.Domain;

namespace Deep.Common.Application.SimpleMediatR;

public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> inner
) : IRequestHandler<TRequest, TResponse>
    where TRequest : class
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        bool isValid = Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true
        );

        if (!isValid)
        {
            Error[] errors = validationResults
                .Where(r => r.ErrorMessage is not null)
                .Select(r => Error.Problem(
                    string.Join(".", r.MemberNames),
                    r.ErrorMessage!
                ))
                .ToArray();

            return Result<TResponse>.ValidationFailure(new ValidationError(errors));
        }

        return await inner.Handle(request, cancellationToken);
    }
}
