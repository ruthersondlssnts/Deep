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
    .AddRedis("redis")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> deepDb = postgres.AddDatabase("deep-db");

IResourceBuilder<MongoDBDatabaseResource> deepMongoDb = mongo.AddDatabase("deep-docs");

builder
    .AddProject<Projects.Deep_Api>("deep-api")
    .WithReference(deepMongoDb)
    .WithReference(deepDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WaitFor(deepMongoDb)
    .WaitFor(deepDb)
    .WaitFor(rabbitmq)
    .WaitFor(redis);

builder.Build().Run();
