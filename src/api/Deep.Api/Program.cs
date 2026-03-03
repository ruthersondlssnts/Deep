using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.BackgroundJobs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddModuleConfiguration(["accounts", "programs", "transactions"]);

string sql = "deep-db";
string mongo = "deep-docs";
string broker = "broker";

string databaseConnectionString = builder.Configuration.GetConnectionString(sql)!;
string mqConnectionString = builder.Configuration.GetConnectionString(broker)!;

builder
    .Services.AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddHangfireInternal(builder.Configuration)
    .AddModules(databaseConnectionString, mqConnectionString, builder.Configuration);

builder.ApplyAspire(sql, mongo, broker);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
    app.MapOpenApi();
    app.UseHangfireInternal(enableDashboard: true);
}
else
{
    app.UseHangfireInternal(enableDashboard: false);
}

app.UseInboxOutboxJobs();
app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();

await app.RunAsync();
