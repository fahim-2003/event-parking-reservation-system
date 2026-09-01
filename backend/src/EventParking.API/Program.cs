using EventParking.API.Configurations;
using EventParking.API.Data;
using EventParking.API.Extensions;
using EventParking.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity
builder.Services.AddApplicationIdentity();

// Strongly typed configuration
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<BookingSettings>(
    builder.Configuration.GetSection(BookingSettings.SectionName));

builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));

// Standard API error responses
builder.Services.AddProblemDetails();

// CORS configuration for Angular frontend
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// API services
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Allow configured Angular frontend
app.UseCors("Frontend");

app.UseHttpsRedirection();

// Authentication middleware will be added in the JWT wiring step.
app.UseAuthorization();

app.MapControllers();

// API health endpoint
app.MapHealthChecks("/api/health");

app.Run();