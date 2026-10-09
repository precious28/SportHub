using Microsoft.EntityFrameworkCore;
using SportHub.Application.Interfaces;
using SportHub.Application.Services;
using SportHub.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// MySQL + EF Core
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<SportHubDbContext>(opt =>
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IFixtureRepository, FixtureRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Application Services
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<FixtureService>();

// MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SportHubDbContext>();
    db.Database.Migrate();
}

app.MapMcp();
app.Run();
