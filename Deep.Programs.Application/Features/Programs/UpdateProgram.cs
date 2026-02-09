using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
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
    public sealed record ProgramUser(Guid UserId, string RoleName);

    public sealed record Command(
        Guid ProgramId,
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        IReadOnlyCollection<string> ProductNames,
        IReadOnlyCollection<ProgramUser> Users
    );

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Description).NotEmpty();

            RuleFor(x => x.StartsAtUtc)
                .Must(x => x > DateTime.UtcNow)
                .WithMessage("Start date must be in the future.");

            RuleFor(x => x.EndsAtUtc)
                .GreaterThan(x => x.StartsAtUtc)
                .WithMessage("End date must be after start date.");

            RuleFor(x => x.ProductNames)
                .NotEmpty()
                .WithMessage("At least one product is required.");

            RuleFor(x => x.Users).NotEmpty().WithMessage("At least one user is required.");

            RuleForEach(x => x.Users)
                .ChildRules(user =>
                {
                    user.RuleFor(u => u.UserId).NotEmpty();

                    user.RuleFor(u => u.RoleName).NotEmpty();
                });
        }
    }

    public sealed class Handler(ProgramsDbContext context, ProgramRepository programRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            Program? program = await context
                .Programs.Include(p => p.Products)
                .SingleOrDefaultAsync(p => p.Id == c.ProgramId, ct);

            if (program is null)
            {
                return ProgramErrors.NotFound(c.ProgramId);
            }

            var assignmentPairs = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            if (!await AreAllUsersValid(assignmentPairs, programRepository))
            {
                return ProgramErrors.ProgramUserNotFound;
            }

            ProgramUpdateResult update = program.UpdateDetails(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.ProductNames,
                assignmentPairs,
                await GetExistingAssignments(program.Id, ct)
            );

            if (update.Result.IsFailure)
            {
                return update.Result.Error;
            }

            if (update.NewAssignments.Count > 0)
            {
                context.ProgramAssignments.AddRange(update.NewAssignments);
            }

            await context.SaveChangesAsync(ct);
            return new Response(program.Id);
        }

        private async Task<bool> AreAllUsersValid(
            List<(Guid UserId, string RoleName)> assignmentPairs,
            ProgramRepository programRepository
        )
        {
            int expected = assignmentPairs.Select(a => a.UserId).Distinct().Count();
            int valid = await programRepository.CountMatchingUserRolePairs(assignmentPairs);
            return valid == expected;
        }

        private Task<List<ProgramAssignment>> GetExistingAssignments(
            Guid programId,
            CancellationToken ct
        ) => context.ProgramAssignments.Where(a => a.ProgramId == programId).ToListAsync(ct);
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPut(
                    "/programs/{programId:guid}",
                    async (
                        Guid programId,
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(
                            command with
                            {
                                ProgramId = programId,
                            },
                            ct
                        );

                        return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
                    }
                )
                .WithTags("Programs");
    }
}
