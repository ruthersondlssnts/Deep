using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddModuleConfiguration(["accounts", "programs", "transactions"]);

string sql = "deep-db";
string mongo = "deep-docs";
string broker = "broker";
string redis = "redis";

string databaseConnectionString = builder.Configuration.GetConnectionString(sql)!;
string mqConnectionString = builder.Configuration.GetConnectionString(broker)!;
string? redisConnectionString = builder.Configuration.GetConnectionString(redis);

builder
    .Services.AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddModules(
        databaseConnectionString,
        mqConnectionString,
        redisConnectionString,
        builder.Configuration
    );

builder.ApplyAspire(sql, mongo, broker, redis);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
    await app.SeedDevelopmentDataAsync();
}
else
{
    app.UseExceptionHandler();
}

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

await app.RunAsync();
