using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Deep.Programs.Application.BackgroundJobs;
using Deep.Programs.Application.Data;
using Deep.Programs.Application.Inbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Deep.Programs.Application;

public static class ProgramsModule
{
    public const string ModuleName = "Programs";

    public static IServiceCollection AddProgramsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Programs)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Programs)
            .AddPostgresDbContext<ProgramsDbContext>(Schemas.Programs, configuration)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddMongoDb<MongoDbContext>(
                "deep",
                () =>
                    BsonSerializer.RegisterSerializer(
                        new GuidSerializer(GuidRepresentation.Standard)
                    )
            )
            .AddOutboxInboxJobs<
                ProgramsProcessOutboxJob,
                ProgramsProcessInboxJob,
                ProgramsInboxWriter
            >();

        services.Configure<OutboxOptions>(configuration.GetSection("Programs:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Programs:Inbox"));

        return services;
    }

    public static void ConfigureConsumers(
        MassTransit.IRegistrationConfigurator registrationConfigurator
    ) =>
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator,
            typeof(ProgramsIntegrationEventConsumer<>)
        );
}
