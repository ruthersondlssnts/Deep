using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Aspire Connections
var postgresDb = "deep-db";
var mongoDb = "deep-docs";
var rabbitmq = "rabbitmq";

var databaseConnectionString = builder.Configuration.GetConnectionString(postgresDb)!;
var mqConnectionString = builder.Configuration.GetConnectionString(rabbitmq)!;

builder
    .Services.AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddModules(databaseConnectionString, mqConnectionString);

builder.ApplyAspire(postgresDb, mongoDb, rabbitmq);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
    app.MapOpenApi();
}
app.MapDefaultEndpoints();
app.MapEndpoints();
app.UseHttpsRedirection();

//app.UseAuthorization();
app.Run();
