using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin => { 
        pgAdmin.WithHostPort(8001);
        pgAdmin.WithLifetime(ContainerLifetime.Persistent);
        pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "PostgreDB Browser");
    })
    .WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var deepdb = postgres.AddDatabase("deep-db");

var mongodb = builder.AddMongoDB("mongo")
    .WithMongoExpress(mongoExpress => {
        mongoExpress.WithHostPort(8002);
        mongoExpress.WithLifetime(ContainerLifetime.Persistent);
        mongoExpress.WithUrlForEndpoint("http", url => url.DisplayText = "MongoDB Browser");
    })
    .WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var deepMongo = mongodb.AddDatabase("deep");

builder.AddProject<Projects.Deep_Api>("deep-api")
    .WithReference(deepMongo)
    .WithReference(deepdb)
    .WaitFor(deepMongo)
    .WaitFor(deepdb);

builder.Build().Run();
