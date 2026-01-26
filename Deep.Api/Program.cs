using Deep.Accounts.Application;
using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;
using Deep.Programs.Application;
using Deep.Transactions.Application;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Aspire Connections
var postgresDb = "deep-db";
var mongoDb = "deep-docs";
var rabbitmq = "rabbitmq";

var databaseConnectionString = builder.Configuration.GetConnectionString(postgresDb)!;
var mqConnectionString = builder.Configuration.GetConnectionString(rabbitmq)!;


builder.Services
    .AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddDapperAndNpgsql(databaseConnectionString)
    .AddCustomMediatR(
        Deep.Programs.Application.AssemblyReference.Assembly,
        Deep.Accounts.Application.AssemblyReference.Assembly,
        Deep.Transactions.Application.AssemblyReference.Assembly)
    .AddMassTransit(
        mqConnectionString,
        [ProgramsModule.ConfigureConsumers,
        AccountsModule.ConfigureConsumers,
        TransactionsModule.ConfigureConsumers]
    );


builder.Services
    .AddProgramsModule()
    .AddAccountsModule()
    .AddTransactionsModule();

builder.ApplyAspire(postgresDb, mongoDb, rabbitmq);


var app = builder.Build();

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
