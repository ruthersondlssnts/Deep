using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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

    public sealed class Handler(
        ProgramsDbContext context,
        IProgramRepository programRepository,
        IUserRepository userRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            Program? program = await programRepository.GetAsync(c.ProgramId, ct);

            if (program is null)
            {
                return ProgramErrors.NotFound(c.ProgramId);
            }

            var assignments = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            if (!await userRepository.ExistWithRolesAsync(assignments, ct))
            {
                return ProgramErrors.ProgramUserNotFound;
            }

            Result updateResult = program.UpdateDetails(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.ProductNames,
                assignments
            );

            if (updateResult.IsFailure)
            {
                return updateResult.Error;
            }

            await context.SaveChangesAsync(ct);

            return new Response(program.Id);
        }
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
