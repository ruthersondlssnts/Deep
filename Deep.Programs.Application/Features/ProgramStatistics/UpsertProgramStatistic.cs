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

public static class UpsertProgramStatistic
{
    public sealed record Command(
        Guid ProgramId,
        string? Name = null,
        string? Description = null,
        ProgramStatus? ProgramStatus = null,
        DateTime? StartsAtUtc = null,
        DateTime? EndsAtUtc = null,
        Guid? OwnerId = null,
        string? Owner = null,
        int? TotalCoordinators = null,
        int? TotalBrandAmbassadors = null,
        int? TotalTransactions = null,
        int? TotalCustomers = null);
    public sealed record Response(Guid Id);

    public sealed class Handler(
        MongoDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
        Command request,
        CancellationToken ct)
        {
            var u = Builders<ProgramStatistic>.Update;
            var updates = new List<UpdateDefinition<ProgramStatistic>>();

            if (request.Name is not null)
                updates.Add(u.Set(x => x.Name, request.Name));

            if (request.Description is not null)
                updates.Add(u.Set(x => x.Description, request.Description));

            if (request.ProgramStatus is not null)
            {
                updates.Add(u.Set(x => x.ProgramStatus, request.ProgramStatus.Value));
                updates.Add(u.Set(x => x.ProgramStatusName, request.ProgramStatus.Value.ToString()));
            }

            if (request.StartsAtUtc is not null)
                updates.Add(u.Set(x => x.StartsAtUtc, request.StartsAtUtc.Value));

            if (request.EndsAtUtc is not null)
                updates.Add(u.Set(x => x.EndsAtUtc, request.EndsAtUtc.Value));

            if (request.OwnerId is not null)
                updates.Add(u.Set(x => x.OwnerId, request.OwnerId.Value));

            if (request.Owner is not null)
                updates.Add(u.Set(x => x.Owner, request.Owner));

            if (request.TotalCoordinators is not null)
                updates.Add(u.Set(x => x.TotalCoordinators, request.TotalCoordinators.Value));

            if (request.TotalBrandAmbassadors is not null)
                updates.Add(u.Set(x => x.TotalBrandAmbassadors, request.TotalBrandAmbassadors.Value));

            if (request.TotalTransactions is not null)
                updates.Add(u.Set(x => x.TotalTransactions, request.TotalTransactions.Value));

            if (request.TotalCustomers is not null)
                updates.Add(u.Set(x => x.TotalCustomers, request.TotalCustomers.Value));

            // defaults on insert
            updates.Add(u.SetOnInsert(x => x.ProgramId, request.ProgramId));

            if (request.TotalTransactions is null)
                updates.Add(u.SetOnInsert(x => x.TotalTransactions, 0));

            if (request.TotalCustomers is null)
                updates.Add(u.SetOnInsert(x => x.TotalCustomers, 0));

            if (updates.Count == 0)
                return new Response(request.ProgramId);

            await context.ProgramStatistics.UpdateOneAsync(
                x => x.ProgramId == request.ProgramId,
                u.Combine(updates),
                new UpdateOptions { IsUpsert = true },
                ct);

            return new Response(request.ProgramId);
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost("/program-statistics/{programId:guid}", async (
                Guid programId,
                Command command,
                IRequestHandler<Command, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(
                    command with { ProgramId = programId }, ct);

                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem);
            })
            .WithTags("ProgramStatistics");
    }
}
