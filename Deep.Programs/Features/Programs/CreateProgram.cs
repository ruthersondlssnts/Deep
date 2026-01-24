using Deep.Common.Api.ApiResults;
using Deep.Common.Api.Endpoints;
using Deep.Common.Domain;
using Deep.Common.SimpleMediatR;
using Deep.Programs.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Features.Programs;
public static class CreateProgram
{
    public sealed record Command(
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        IReadOnlyCollection<string> ProductNames,
        IReadOnlyCollection<Guid> UserIds);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.StartsAtUtc).GreaterThan(DateTime.UtcNow);
            RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
            RuleFor(x => x.UserIds).NotEmpty();
            RuleFor(x => x.ProductNames).NotEmpty()
                .WithMessage("At least one product is required.");
        }
    }

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            var users = await context.Users
                .Where(u => c.UserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Role })
                .ToListAsync(ct);

            if (users.Count != c.UserIds.Distinct().Count())
                return ProgramErrors.ProgramUserNotFound;

            var currentUserId = await context.Users //Change to current user in future auth implementation
                .Where(u => u.Role == Role.ProgramOwner)
                .Select(u => u.Id)
                .SingleAsync(ct);

            var assignmentValidation = new AssignmentValidationResult(
                CoordinatorCount: users.Count(u => u.Role == Role.Coordinator),
                CoOwnerCount: users.Count(u => u.Role == Role.ProgramOwner)
            );

            var programResult = Program.Create(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                currentUserId,
                c.ProductNames,
                assignmentValidation);

            if (programResult.IsFailure)
                return programResult.Error;

            var program = programResult.Value;

            var assignments = ProgramAssignment
                .CreateBatch(
                    program.Id,
                    users
                        .Select(u => (UserId: u.Id, u.Role))
                        .ToList());

            context.Programs.Add(program);
            context.ProgramAssignments.AddRange(assignments);
            await context.SaveChangesAsync(ct);

            return new Response(program.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost("/programs", async (
                Command command,
                IRequestHandler<Command, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.Created(
                        $"/programs/{result.Value.Id}",
                        result.Value),
                    ApiResults.Problem);
            })
            .WithTags("Programs");
    }
}
