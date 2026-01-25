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

var databaseConnectionString = builder.Configuration.GetConnectionString("deep-db")!;

builder.Services
    .AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddDapperAndNpgsql(databaseConnectionString)
    .AddCustomMediatR(
        Deep.Programs.Application.AssemblyReference.Assembly,
        Deep.Accounts.Application.AssemblyReference.Assembly,
        Deep.Transactions.Application.AssemblyReference.Assembly)
    .AddMassTransit(
        ProgramsModule.ConfigureConsumers,
        AccountsModule.ConfigureConsumers,
        TransactionsModule.ConfigureConsumers
    );

builder.AddProgramsModule();
builder.AddAccountsModule();
builder.AddTransactionsModule();

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
