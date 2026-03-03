using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Programs.Application.BackgroundJobs;
using Deep.Programs.Application.Data;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Deep.Programs.Application;

public static class ProgramsModule
{
    public const string ModuleName = "Programs";

    public static IServiceCollection AddProgramsModule(this IServiceCollection services)
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Programs)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Programs)
            .AddPostgresDbContextWithSchema<ProgramsDbContext>(Schemas.Programs)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddMongoDb<MongoDbContext>(
                "deep",
                () =>
                    BsonSerializer.RegisterSerializer(
                        new GuidSerializer(GuidRepresentation.Standard)
                    )
            )
            .AddOutboxInterceptor<ProgramsDbContext>()
            .AddOutboxInboxJobs<ProgramsProcessOutboxJob, ProgramsProcessInboxJob, ProgramsInboxWriter>();
        return services;
    }

    public static void ConfigureConsumers(
        MassTransit.IRegistrationConfigurator registrationConfigurator
    ) =>
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator
        );
}
