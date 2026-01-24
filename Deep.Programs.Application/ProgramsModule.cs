using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Database;
using Deep.Common.Application.Messaging;
using Deep.Programs.Application.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Deep.Programs.Application;

public static class ProgramsModule
{
    public static IHostApplicationBuilder AddProgramsModule(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddDomainEventHandlers();
       
        builder.Services
            .AddInfrastructure(builder.Configuration)
            .AddEndpoints(AssemblyReference.Assembly);

        // Aspire enrichment belongs HERE
        builder.EnrichNpgsqlDbContext<ProgramsDbContext>();

        builder.AddMongoDBClient(connectionName: "deep");

        builder.Services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("deep");
        });

        builder.Services.AddSingleton<MongoDbContext>();

        BsonSerializer.RegisterSerializer(
            new GuidSerializer(GuidRepresentation.Standard));

        return builder;
    }

    private static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
            services.AddDatabase(configuration);

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration) =>
             services.AddDbContext<ProgramsDbContext>(
                Postgres.StandardOptions(configuration, Schemas.Programs))
            ;



    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator) => 
        registrationConfigurator.AddConsumers(AssemblyReference.Assembly);

    private static void AddDomainEventHandlers(this IServiceCollection services) =>
       AssemblyReference.Assembly
           .GetTypes()
           .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
           .ToList()
           .ForEach(services.TryAddScoped);
}
