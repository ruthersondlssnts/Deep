
using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Programs.Application.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Deep.Programs.Application;

public static class ProgramsModule
{
    public static IHostApplicationBuilder AddProgramsModule(this IHostApplicationBuilder builder)
    {
        return builder
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<ProgramsDbContext>(builder.Configuration, Schemas.Programs)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddMongoDb<MongoDbContext>("deep", "deep", () => BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard)))
            .AddDomainEventInterceptor<ProgramsDbContext>(AssemblyReference.Assembly);
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        Deep.Common.Application.ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
