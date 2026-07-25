using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentRegistry.API.Middleware;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Application.Mappings;
using StudentRegistry.Application.Services;
using StudentRegistry.Application.Validators;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Interfaces;
using StudentRegistry.Infrastructure.Storage;
using StudentRegistry.Repository.Implementations;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure services
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Health Checks for Monitoring
builder.Services.AddHealthChecks();

// Configure SQL Server 2017 DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StudentRegistryDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.MigrationsAssembly("StudentRegistry.Data")));

// Configure Dependency Injection Layers
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(StudentCreateDtoValidator).Assembly);

// Configure Cookie Policy for Production Security
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

// 2. Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Inject custom security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Enable secure cookie policies
app.UseCookiePolicy();

// Global Exception Handler Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Enable serving uploads folder static files (e.g. localhost:5000/uploads/file.png)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Map Health Checks Endpoint
app.MapHealthChecks("/health");

// Ensure wwwroot/uploads directory exists on startup
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.Run();
