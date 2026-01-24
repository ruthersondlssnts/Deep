using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;

namespace Deep.Programs.Application.Features.ProgramStatistics;

public static class GetProgramStatistic
{
    public sealed record Query(Guid ProgramId);

    public sealed record Response(
        Guid ProgramId,
        string Name,
        string Description,
        ProgramStatus ProgramStatus,
        string ProgramStatusName,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        Guid OwnerId,
        string FullName,
        int TotalCoordinators,
        int TotalBrandAmbassadors,
        int TotalTransactions);


    public sealed class Handler(
        MongoDbContext context)
        : IRequestHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(
            Query request,
            CancellationToken ct)
        {
            var stat = await context.ProgramStatistics
                .Find(x => x.ProgramId == request.ProgramId)
                .FirstOrDefaultAsync(ct);

            if (stat is null)
                return Error.NotFound("ProgramStatistic.NotFound", $"The program statistic with the identifier {request.ProgramId} was not found");

            return new Response(
                stat.ProgramId,
                stat.Name,
                stat.Description,
                stat.ProgramStatus,
                stat.ProgramStatusName,
                stat.StartsAtUtc,
                stat.EndsAtUtc,
                stat.OwnerId,
                stat.Owner,
                stat.TotalCoordinators,
                stat.TotalBrandAmbassadors,
                stat.TotalTransactions);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet("/program-statistics/{programId:guid}", async (
                Guid programId,
                IRequestHandler<Query, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(
                    new Query(programId), ct);

                return result.Match(
                    Results.Ok,
                    ApiResults.Problem);
            })
            .WithTags("ProgramStatistics");
    }
}
