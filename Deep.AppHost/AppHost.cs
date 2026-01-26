var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin(pgAdmin =>
    {
        pgAdmin.WithHostPort(8001);
        pgAdmin.WithLifetime(ContainerLifetime.Persistent);
        pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "PostgreSQL Browser");
    })
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongo = builder.AddMongoDB("mongo")
    .WithMongoExpress(mongoExpress =>
    {
        mongoExpress.WithHostPort(8002);
        mongoExpress.WithLifetime(ContainerLifetime.Persistent);
        mongoExpress.WithUrlForEndpoint("http", url => url.DisplayText = "MongoDB Browser");
    })
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin(8003)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var deepDb = postgres.AddDatabase("deep-db");

var deepMongoDb = mongo.AddDatabase("deep-docs");

builder.AddProject<Projects.Deep_Api>("deep-api")
    .WithReference(deepMongoDb)
    .WithReference(deepDb)
    .WithReference(rabbitmq)
    .WaitFor(deepMongoDb)
    .WaitFor(deepDb)
    .WaitFor(rabbitmq);

builder.Build().Run();
