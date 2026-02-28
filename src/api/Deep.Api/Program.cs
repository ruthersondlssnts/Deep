using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Aspire Connections
string sql = "deep-db";
string mongo = "deep-docs";
string broker = "broker";

string databaseConnectionString = builder.Configuration.GetConnectionString(sql)!;
string mqConnectionString = builder.Configuration.GetConnectionString(broker)!;

builder
    .Services.AddValidation()
    .AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddModules(databaseConnectionString, mqConnectionString);

builder.ApplyAspire(sql, mongo, broker);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
    app.MapOpenApi();
}
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();
