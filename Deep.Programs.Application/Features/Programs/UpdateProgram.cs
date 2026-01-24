using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Domain;
using Deep.Common.Application.SimpleMediatR;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public static class UpdateProgram
{
    public sealed record Command(
        Guid ProgramId,
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        IReadOnlyCollection<string> Products,
        IReadOnlyCollection<Guid> UserIds);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProgramId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.StartsAtUtc).GreaterThan(DateTime.UtcNow);
            RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
            RuleFor(x => x.UserIds).NotEmpty();
            RuleFor(x => x.Products)
                .NotEmpty()
                .WithMessage("At least one product is required.");
        }
    }

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            var program = await context.Programs
                .Include(p => p.Products)
                .SingleOrDefaultAsync(p => p.Id == c.ProgramId, ct);

            if (program is null)
                return ProgramErrors.NotFound(c.ProgramId);

            var users = await context.Users
                .Where(u => c.UserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Role })
                .ToListAsync(ct);

            if (users.Count != c.UserIds.Distinct().Count())
                return ProgramErrors.ProgramUserNotFound;

            var assignments = await context.ProgramAssignments
                .Where(a => a.ProgramId == program.Id)
                .ToListAsync(ct);

            var assignmentValidation = new AssignmentValidationResult(
                CoordinatorCount: users.Count(u => u.Role == Role.Coordinator),
                CoOwnerCount: users.Count(u => u.Role == Role.ProgramOwner)
            );

            program.UpdateDetails(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.Products,
                assignmentValidation);

            var newAssignments = ProgramAssignment.Sync(
                program.Id,
                assignments,
                users.Select(u => (u.Id, u.Role)).ToList());

            context.ProgramAssignments.AddRange(newAssignments);

            await context.SaveChangesAsync(ct);

            return new Response(program.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPut("/programs/{programId:guid}", async (
                Guid programId,
                Command command,
                IRequestHandler<Command, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(
                    command with { ProgramId = programId },
                    ct);

                return result.Match(
                    () => Results.Ok(result.Value),
                    ApiResults.Problem);
            })
            .WithTags("Programs");
    }
}
