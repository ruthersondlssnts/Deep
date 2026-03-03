using Deep.Accounts.Application;
using Deep.Accounts.Application.BackgroundJobs;
using Deep.Api.Extensions;
using Deep.Common.Application.BackgroundJobs;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Deep.Programs.Application;
using Deep.Programs.Application.BackgroundJobs;
using Deep.Transactions.Application;
using Deep.Transactions.Application.BackgroundJobs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Aspire Connections
string sql = "deep-db";
string mongo = "deep-docs";
string broker = "broker";

string databaseConnectionString = builder.Configuration.GetConnectionString(sql)!;
string mqConnectionString = builder.Configuration.GetConnectionString(broker)!;

builder
    .Services.AddOpenApiAndSwagger()
    .AddExceptionAndProblemDetails()
    .AddModules(databaseConnectionString, mqConnectionString);

// Configure Hangfire for background job processing
builder.Services.AddHangfireInternal(builder.Configuration);

// Configure Outbox and Inbox options
builder.Services
    .AddOptions<OutboxOptions>()
    .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<InboxOptions>()
    .Bind(builder.Configuration.GetSection(InboxOptions.SectionName))
    .ValidateDataAnnotations();

builder.ApplyAspire(sql, mongo, broker);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
    app.MapOpenApi();

    // Enable Hangfire dashboard in development
    app.UseHangfireInternal(enableDashboard: true);
}
else
{
    app.UseHangfireInternal(enableDashboard: false);
}

// Register inbox/outbox recurring jobs for all modules
app.UseInboxOutboxJobs<AccountsProcessOutboxJob, AccountsProcessInboxJob>(AccountsModule.ModuleName);
app.UseInboxOutboxJobs<ProgramsProcessOutboxJob, ProgramsProcessInboxJob>(ProgramsModule.ModuleName);
app.UseInboxOutboxJobs<TransactionsProcessOutboxJob, TransactionsProcessInboxJob>(TransactionsModule.ModuleName);

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();
