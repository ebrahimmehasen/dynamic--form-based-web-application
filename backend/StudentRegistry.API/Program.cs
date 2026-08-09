using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
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

// Bootstrap logger — active only until the full Serilog pipeline (registered below via
// UseSerilog) takes over, so that a crash during host/DI setup itself (bad connection string,
// missing config, DI resolution failure...) still gets written to the log file instead of being
// lost. Same file target as the full pipeline, so bootstrap-phase and steady-state logs land in
// the same daily file.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateBootstrapLogger();

try
{
    Log.Information("جاري بدء تشغيل التطبيق...");

    var builder = WebApplication.CreateBuilder(args);

    // Full logging pipeline: every log statement in the app (framework + our own ILogger<T> calls,
    // including the global ExceptionMiddleware below) flows through here — to the console (visible
    // while the process is attached to a terminal) and to a rolling daily file under Logs/, so
    // production failures (DB, file storage, PDF export, auth, unhandled exceptions, every HTTP
    // request/response) are all recoverable after the fact, not just while someone is watching the
    // console. EF Core's per-query SQL command logging is dropped down to Warning so the file
    // doesn't fill up with routine SELECT/INSERT statements — only slow/failing queries and actual
    // errors get through at their natural level.
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"));

// 1. Configure services
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<IEligibilityExportService, EligibilityExportService>();
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

// One structured log line per HTTP request/response (method, path, status code, elapsed time) —
// placed first so it wraps every other middleware below, including auth challenges and the global
// exception handler. This alone answers "what was the site doing right before it failed" without
// needing to reproduce the failure: every request that ever hit the server, successful or not, is
// in the log file.
app.UseSerilogRequestLogging();

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

// Startup database connectivity check — logged explicitly (not left to whichever request happens
// to hit the DB first) so a broken connection string, unreachable SQL Server, or expired
// credentials shows up immediately and unambiguously at the top of the log file instead of being
// buried in the first user's failed request. Deliberately non-fatal: the app still starts and
// serves static content/the registration form's UI shell even if the database is down, since the
// database can come back later without a restart.
using (var dbCheckScope = app.Services.CreateScope())
{
    try
    {
        var dbContext = dbCheckScope.ServiceProvider.GetRequiredService<StudentRegistryDbContext>();
        if (await dbContext.Database.CanConnectAsync())
        {
            Log.Information("تم الاتصال بقاعدة البيانات بنجاح.");
        }
        else
        {
            Log.Error("تعذر الاتصال بقاعدة البيانات عند بدء التشغيل — سلسلة الاتصال قد تكون خاطئة أو السيرفر غير متاح.");
        }
    }
    catch (Exception dbEx)
    {
        Log.Error(dbEx, "حدث استثناء أثناء فحص الاتصال بقاعدة البيانات عند بدء التشغيل.");
    }
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

Log.Information("اكتمل بدء تشغيل التطبيق بنجاح.");

    app.Run();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException)
{
    // Deliberately thrown by EF Core's own design-time tooling (dotnet ef migrations/database
    // update) — it builds the host just far enough to discover the DbContext, then aborts on
    // purpose before Run() so the CLI command doesn't actually start serving requests. Expected,
    // not a real failure; must not be logged as Fatal or it looks like the app crashed every time
    // someone runs a migration command.
}
catch (Exception ex)
{
    // Anything else that escapes to here happened before/outside the request pipeline (so
    // ExceptionMiddleware never saw it) — bad config, DI resolution failure, the port already in
    // use, etc. Fatal, because the process is about to exit; this is the only record of why.
    Log.Fatal(ex, "فشل تشغيل التطبيق بشكل غير متوقع.");
}
finally
{
    // Flush any buffered log events to disk/console before the process exits — without this, the
    // last few log lines (often the most important ones, right before a crash) can be lost.
    Log.CloseAndFlush();
}
