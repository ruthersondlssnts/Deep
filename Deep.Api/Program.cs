using Deep.Accounts;
using Deep.Api.Extensions;
using Deep.Transactions.Application;
using Deep.Accounts.Application;
using Deep.Programs.Application;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Application.EventBus;
using Deep.Common.Api.Middleware;
using Deep.Common.Application.Api.Endpoints;

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
    Deep.Programs.Application.AssemblyReference.Assembly, 
    Deep.Accounts.Application.AssemblyReference.Assembly,
    Deep.Transactions.Application.AssemblyReference.Assembly], 
    includeInternalTypes: true);

builder.Services.AddScoped<IRequestBus, RequestBus>();

//Simple custom MediatR registrations
builder.Services.AddRequestHandlers([
    Deep.Programs.Application.AssemblyReference.Assembly, 
    Deep.Accounts.Application.AssemblyReference.Assembly,
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
