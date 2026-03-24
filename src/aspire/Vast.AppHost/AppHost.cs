IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithPgAdmin(pgAdmin =>
    {
        pgAdmin.WithHostPort(8001);
        pgAdmin.WithLifetime(ContainerLifetime.Persistent);
        pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "PostgreSQL Browser");
    })
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<MongoDBServerResource> mongo = builder
    .AddMongoDB("mongo")
    .WithMongoExpress(mongoExpress =>
    {
        mongoExpress.WithHostPort(8002);
        mongoExpress.WithLifetime(ContainerLifetime.Persistent);
        mongoExpress.WithUrlForEndpoint("http", url => url.DisplayText = "MongoDB Browser");
    })
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<RabbitMQServerResource> rabbitmq = builder
    .AddRabbitMQ("broker")
    .WithManagementPlugin(8003)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<RedisResource> redis = builder
    .AddRedis("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> VastDb = postgres.AddDatabase("vast-db");

IResourceBuilder<MongoDBDatabaseResource> VastMongoDb = mongo.AddDatabase("vast-docs");

builder
    .AddProject<Projects.Vast_Api>("vast-api")
    .WithReference(VastMongoDb)
    .WithReference(VastDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(VastMongoDb)
    .WaitFor(VastDb)
    .WaitFor(rabbitmq)
    .WaitFor(redis);

await builder.Build().RunAsync();
