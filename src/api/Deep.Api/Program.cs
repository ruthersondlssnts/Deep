using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.BackgroundJobs;

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
    .AddHangfireInternal(builder.Configuration)
    .AddModules(databaseConnectionString, mqConnectionString, redisConnectionString, builder.Configuration);

builder.ApplyAspire(sql, mongo, broker, redis);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
    await app.SeedDevelopmentDataAsync();
    app.UseHangfireInternal(enableDashboard: true);
}
else
{
    app.UseHangfireInternal(enableDashboard: false);
    app.UseExceptionHandler();
}

app.UseInboxOutboxJobs();
app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

await app.RunAsync();
