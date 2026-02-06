// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Api.Extensions;
using Deep.Common.Application.Api.Endpoints;

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
    .AddModules(databaseConnectionString, mqConnectionString);

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
