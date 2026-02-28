using System.Diagnostics;
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

        if (
            problemDetails is HttpValidationProblemDetails validationProblemDetails
            && validationProblemDetails.Errors.Count > 0
        )
        {
            List<Error> errors = [];
            foreach (KeyValuePair<string, string[]> kvp in validationProblemDetails.Errors)
            {
                foreach (string message in kvp.Value)
                {
                    errors.Add(Error.Problem(kvp.Key, message));
                }
            }

            ValidationError validationError = new(errors.ToArray());
            var result = Result.Failure(validationError);

            problemDetails.Type = ApiResults.ApiResults.GetType(result.Error.Type);
            problemDetails.Title = ApiResults.ApiResults.GetTitle(result.Error);
            problemDetails.Status = ApiResults.ApiResults.GetStatusCode(result.Error.Type);
            problemDetails.Detail = ApiResults.ApiResults.GetDetail(result.Error);

            validationProblemDetails.Errors.Clear();
            Dictionary<string, object?>? extensions = ApiResults.ApiResults.GetErrors(result);
            if (extensions is not null)
            {
                foreach (KeyValuePair<string, object?> kvp in extensions)
                {
                    problemDetails.Extensions[kvp.Key] = kvp.Value;
                }
            }

            string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;
        }

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status400BadRequest;

        return new ValueTask(
            httpContext.Response.WriteAsJsonAsync(problemDetails, httpContext.RequestAborted)
        );
    }
}
