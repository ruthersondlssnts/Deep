using Deep.Accounts;
using Deep.Api.Extensions;
using Deep.Common.Api.Endpoints;
using Deep.Common.Api.Middleware;
using Deep.Common.Dapper;
using Deep.Common.EventBus;
using Deep.Common.Messaging;
using Deep.Common.SimpleMediatR;
using Deep.Programs;
using Deep.Transactions.Application;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
});
var databaseConnectionString =
    builder.Configuration.GetConnectionString("deep-db")!;
var mongodbConnectionString =
    builder.Configuration.GetConnectionString("deep")!;

var npgsqlDataSource =
    new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

builder.Services.TryAddSingleton(npgsqlDataSource);

builder.AddProgramsModule();
builder.AddAccountsModule();
builder.AddTransactionsModule();
builder.Services.AddValidatorsFromAssemblies([
    Deep.Programs.AssemblyReference.Assembly, 
    Deep.Accounts.AssemblyReference.Assembly,
    Deep.Transactions.Application.AssemblyReference.Assembly], 
    includeInternalTypes: true);

builder.Services.AddScoped<IRequestBus, RequestBus>();

//Simple custom MediatR registrations
builder.Services.AddRequestHandlers([
    Deep.Programs.AssemblyReference.Assembly, 
    Deep.Accounts.AssemblyReference.Assembly,
    Deep.Transactions.Application.AssemblyReference.Assembly]);
builder.Services.AddRequestPipelines(
    typeof(ValidationPipelineBehavior<,>),
    typeof(RequestLoggingPipelineBehavior<,>),
    typeof(ExceptionHandlingPipelineBehavior<,>));

builder.Services.TryAddSingleton<IEventBus, EventBus>();

builder.Services.AddMassTransit(configurator =>
{
    foreach (var configureConsumer in new Action<IRegistrationConfigurator>[]
       {
            ProgramsModule.ConfigureConsumers,
            AccountsModule.ConfigureConsumers,
            TransactionsModule.ConfigureConsumers,
       })
    {
        configureConsumer(configurator);
    }
    configurator.SetKebabCaseEndpointNameFormatter();
    configurator.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});

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
