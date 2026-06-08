using Microsoft.EntityFrameworkCore;
using PolicyManagement.API.Middleware;
using PolicyManagement.Application.Interfaces;
using PolicyManagement.Application.Mappings;
using PolicyManagement.Application.Services;
using PolicyManagement.Infrastructure.Persistence;
using PolicyManagement.Infrastructure.Persistence.Repositories;
using PolicyManagement.Infrastructure.Seeders;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseSqlite("Data Source=PolicyManagement.db"));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PolicyMappingProfile>());

builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Policy Management API", Version = "v1" }));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
    await PolicySeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
