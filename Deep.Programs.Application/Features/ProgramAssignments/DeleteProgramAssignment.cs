using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.ProgramAssignments;

public static class DeleteProgramAssignment
{
    public sealed record Command(Guid AssignmentId);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.AssignmentId).NotEmpty();
        }
    }

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken ct)
        {
            var assignment = await context.ProgramAssignments
                .FirstOrDefaultAsync(a => a.Id == c.AssignmentId, ct);

            if (assignment is null)
                return ProgramErrors.NotFound(c.AssignmentId);

            assignment.Deactivate();

            await context.SaveChangesAsync(ct);

            return new Response(assignment.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapDelete("/program-assignments/{id:guid}", async (
                Guid id,
                IRequestHandler<Command, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(
                    new Command(id), ct);

                return result.Match(
                    Results.Ok,
                    ApiResults.Problem);
            })
            .WithTags("ProgramAssignments");
    }
}

