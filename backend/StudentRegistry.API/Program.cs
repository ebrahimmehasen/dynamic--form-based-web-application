using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentRegistry.API.Middleware;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Application.Mappings;
using StudentRegistry.Application.Services;
using StudentRegistry.Application.Validators;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using StudentRegistry.Infrastructure.Export;
using StudentRegistry.Infrastructure.Storage;
using StudentRegistry.Repository.Implementations;
using System;
using System.IO;
using System.Threading.Tasks;

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IReviewNoteService, ReviewNoteService>();
builder.Services.AddScoped<IStudentExcelExportService, StudentExcelExportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFieldEditService, FieldEditService>();
builder.Services.AddScoped<IFieldCommentService, FieldCommentService>();
builder.Services.AddScoped<IDeleteRequestService, DeleteRequestService>();
builder.Services.AddScoped<IPendingReviewService, PendingReviewService>();
builder.Services.AddScoped<IEditorStudentService, EditorStudentService>();
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

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

// Cookie-based authentication for the internal (non-public) pages/APIs — the public registration
// form/API stay anonymous. Unauthenticated/unauthorized requests to /api/* get a plain 401/403
// instead of the default HTML redirect-to-login, since those are called from fetch(), not a browser
// navigation; everything else (Razor Pages) gets the normal redirect-to-/login behavior.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "StudentRegistry.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
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

// Seed the 3 test users (viewer/editor/admin, password "1234") on first run only — never
// overwrites or duplicates existing accounts. Passwords are always stored hashed
// (PasswordHasher<User>), never in plain text.
using (var seedScope = app.Services.CreateScope())
{
    var unitOfWork = seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var passwordHasher = seedScope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    if (!await unitOfWork.Users.AnyAsync())
    {
        var seedUsers = new[]
        {
            new User { Username = "viewer", Role = AuthConstants.RoleViewer },
            new User { Username = "editor", Role = AuthConstants.RoleEditor },
            new User { Username = "admin", Role = AuthConstants.RoleAdmin }
        };

        foreach (var seedUser in seedUsers)
        {
            seedUser.PasswordHash = passwordHasher.HashPassword(seedUser, "1234");
            await unitOfWork.Users.AddAsync(seedUser);
        }

        await unitOfWork.CompleteAsync();
    }

    // Root admin account, seeded independently of the block above (so it's created even on a
    // database that already has the viewer/editor/admin test users). Protected: its username,
    // password and role can never be changed through the Admin UI (see UserManagementService).
    const string rootAdminUsername = "Mohamed";
    if (await unitOfWork.Users.GetByUsernameAsync(rootAdminUsername) == null)
    {
        var rootAdmin = new User
        {
            Username = rootAdminUsername,
            Role = AuthConstants.RoleAdmin,
            IsProtected = true
        };
        rootAdmin.PasswordHash = passwordHasher.HashPassword(rootAdmin, "MohamedHosni_2026");
        await unitOfWork.Users.AddAsync(rootAdmin);
        await unitOfWork.CompleteAsync();
    }
}

app.Run();
