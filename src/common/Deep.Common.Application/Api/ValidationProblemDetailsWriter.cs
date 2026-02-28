using Deep.Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deep.Common.Application.Api;

public sealed class ValidationProblemDetailsWriter : IProblemDetailsService
{
    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        HttpContext httpContext = context.HttpContext;
        ProblemDetails problemDetails = context.ProblemDetails;

        // Check if this is HttpValidationProblemDetails (from built-in validation)
        if (problemDetails is HttpValidationProblemDetails validationProblemDetails &&
            validationProblemDetails.Errors.Count > 0)
        {
            List<Error> errors = [];

            foreach (KeyValuePair<string, string[]> kvp in validationProblemDetails.Errors)
            {
                foreach (string message in kvp.Value)
                {
                    errors.Add(Error.Problem(kvp.Key, message));
                }
            }

            if (errors.Count > 0)
            {
                // Create ValidationError like ValidationPipelineBehavior does
                ValidationError validationError = new(errors.ToArray());

                // Clear the original errors and add formatted ones to extensions
                validationProblemDetails.Errors.Clear();
                problemDetails.Extensions["errors"] = validationError.Errors;
            }
        }

        // Set consistent type URL for validation errors
        problemDetails.Type = problemDetails.Status switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status403Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            StatusCodes.Status422UnprocessableEntity => "https://datatracker.ietf.org/doc/html/rfc4918/#section-11.2",
            _ => problemDetails.Type
        };

        // Update title to match your pattern
        if (problemDetails.Status == StatusCodes.Status400BadRequest)
        {
            problemDetails.Title = "Validation";
        }

        httpContext.Response.ContentType = "application/problem+json";
        return new ValueTask(httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            httpContext.RequestAborted
        ));
    }
}
