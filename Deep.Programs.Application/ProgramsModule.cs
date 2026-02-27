using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Programs.Application.Data;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Deep.Programs.Application;

public static class ProgramsModule
{
    public static IServiceCollection AddProgramsModule(this IServiceCollection services)
    {
        services
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<ProgramsDbContext>(Schemas.Programs)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddMongoDb<MongoDbContext>(
                "deep",
                () =>
                    BsonSerializer.RegisterSerializer(
                        new GuidSerializer(GuidRepresentation.Standard)
                    )
            )
            .AddDomainEventInterceptor<ProgramsDbContext>(AssemblyReference.Assembly);
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
