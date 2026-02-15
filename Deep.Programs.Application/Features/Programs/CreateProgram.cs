using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Deep.Programs.Application.Features.Programs;

public static class CreateProgram
{
    public sealed record ProgramUser(Guid UserId, string RoleName);

    public sealed record Command(
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

    public sealed class Handler(
        ProgramsDbContext context,
        IProgramRepository programRepository,
        IProgramAssignmentRepository programAssignmentRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            User? currentUserEntity = await context
                .Users.Include(u => u.Roles)
                .SingleOrDefaultAsync(u => u.Roles.Any(r => r.Name == Role.ProgramOwner.Name), ct);

            if (currentUserEntity is null)
            {
                return UserErrors.UserRoleNotFound(Guid.Empty, Role.ProgramOwner.Name);
            }

            var assignmentPairs = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            Result<Program> program = Program.Create(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.ProductNames,
                currentUserEntity.Id,
                assignmentPairs
            );

            if (!program.IsSuccess)
            {
                return program.Error;
            }

            Result<IEnumerable<ProgramAssignment>> programAssignmentsResult =
                ProgramAssignment.CreateRange(program.Value.Id, assignmentPairs);

            if (!programAssignmentsResult.IsSuccess)
            {
                return programAssignmentsResult.Error;
            }

            IEnumerable<ProgramAssignment> programAssignments = programAssignmentsResult.Value;

            programRepository.Insert(program);
            programAssignmentRepository.InsertRange(programAssignments);

            await context.SaveChangesAsync(ct);

            return new Response(program.Value.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/programs",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

                        return result.Match(
                            () => Results.Created($"/programs/{result.Value.Id}", result.Value),
                            ApiResults.Problem
                        );
                    }
                )
                .WithTags("Programs");
    }
}
