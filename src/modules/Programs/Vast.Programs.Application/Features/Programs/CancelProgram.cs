using System.ComponentModel.DataAnnotations;
using Vast.Common.Application.Api.ApiResults;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Data;
using Vast.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vast.Programs.Application.Features.Programs;

public static class CancelProgram
{
    public sealed record Command([Required] Guid ProgramId, [Required] string Reason);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, bool>
    {
        public async Task<Result<bool>> Handle(
            Command command,
            CancellationToken cancellationToken = default
        )
        {
            Program? program = await context.Programs.FirstOrDefaultAsync(
                p => p.Id == command.ProgramId,
                cancellationToken
            );

            if (program is null)
            {
                return ProgramErrors.NotFound(command.ProgramId);
            }

            Result result = program.Cancel(command.Reason);

            if (result.IsFailure)
            {
                return result.Error;
            }

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/programs/{programId:guid}/cancel",
                    async (
                        Guid programId,
                        CancelRequest request,
                        IRequestHandler<Command, bool> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<bool> result = await handler.Handle(
                            new Command(programId, request.Reason),
                            ct
                        );

                        return result.Match(
                            () => Results.Ok(new { message = "Program cancelled successfully" }),
                            ApiResults.Problem
                        );
                    }
                )
                .WithTags("Programs");
    }

    public sealed record CancelRequest([Required] string Reason);
}
