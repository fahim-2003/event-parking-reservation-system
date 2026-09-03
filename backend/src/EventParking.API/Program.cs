using EventParking.API.Interfaces.Services;
using EventParking.API.Services;
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

// JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Application services
builder.Services.AddApplicationServices();

// Strongly typed configuration
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<BookingSettings>(
    builder.Configuration.GetSection(BookingSettings.SectionName));

builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));

builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(AdminSeedOptions.SectionName));

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

// Application services
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IParkingService, ParkingService>();

// API services
builder.Services.AddControllers();

// Swagger / OpenAPI with JWT bearer support
builder.Services.AddSwaggerWithJwtAuthentication();

// Health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed Customer / Administrator roles
// and initial Administrator account
await IdentitySeed.SeedAsync(app.Services);

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/health");

app.Run();
