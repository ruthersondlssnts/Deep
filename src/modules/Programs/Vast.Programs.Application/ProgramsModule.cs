using Vast.Common.Application;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.Outbox;
using Vast.Programs.Application.Data;
using Vast.Programs.Application.Inbox;
using Vast.Programs.Application.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Vast.Programs.Application;

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
            .AddPostgresDbContext<ProgramsDbContext, ProgramsInsertOutboxMessagesInterceptor>(
                Schemas.Programs,
                configuration
            )
            .AddEndpoints(AssemblyReference.Assembly)
            .AddMongoDb<MongoDbContext>(
                "Vast",
                () =>
                    BsonSerializer.RegisterSerializer(
                        new GuidSerializer(GuidRepresentation.Standard)
                    )
            )
            .AddProgramsOutbox()
            .AddProgramsInbox();

        services.Configure<OutboxOptions>(configuration.GetSection("Programs:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Programs:Inbox"));

        return services;
    }

    public static void ConfigureConsumers(
        IRegistrationConfigurator registrationConfigurator,
        string? _ = null
    ) =>
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator,
            typeof(ProgramsIntegrationEventConsumer<>)
        );

    public static IServiceCollection AddProgramsOutbox(this IServiceCollection services)
    {
        services.AddSingleton<ProgramsOutboxNotifier>();
        services.AddScoped<ProgramsOutboxProcessor>();
        services.AddScoped<ProgramsInsertOutboxMessagesInterceptor>();
        services.AddHostedService<ProgramsOutboxBackgroundService>();

        return services;
    }

    public static IServiceCollection AddProgramsInbox(this IServiceCollection services)
    {
        services.AddSingleton<ProgramsInboxNotifier>();
        services.AddScoped<ProgramsInboxProcessor>();
        services.AddHostedService<ProgramsInboxBackgroundService>();

        return services;
    }
}
