using ChitChatApi.Context;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration["CHITCHAT_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Missing connection string. Set ConnectionStrings:Default or CHITCHAT_CONNECTION_STRING.");
}

builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
builder.Services.AddScoped<AppDbContext>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
