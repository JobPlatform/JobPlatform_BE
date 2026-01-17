using System.Security.Claims;
using System.Text;
using Hangfire;
using JobPlatform.Api.Seed;
using JobPlatform.Api.Services;
using JobPlatform.Application;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Infrastructure;
using JobPlatform.Infrastructure.Identity;
using JobPlatform.Infrastructure.Notifications;
using JobPlatform.Infrastructure.Processors;
using JobPlatform.Infrastructure.ReminderJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CHỈ GỌI 1 LẦN
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };

        
        opt.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JWT");
                logger.LogError(ctx.Exception, "JWT auth failed");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("EmployerOnly", p => p.RequireRole("Employer"));
    options.AddPolicy("CandidateOnly", p => p.RequireRole("Candidate"));
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddHangfire(cfg =>
    cfg.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// DI
builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();
builder.Services.AddScoped<IJobMatchingService, JobMatchingService>(); 

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();

builder.Services.AddScoped<IUserEmailProvider, IdentityUserEmailProvider<ApplicationUser>>();

builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();

builder.Services.AddScoped<IInterviewReminderJob, InterviewReminderJob>();


builder.Services.AddApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles(); 

app.UseHangfireDashboard(); // dev , prod nhớ auth

RecurringJob.AddOrUpdate<IOutboxProcessor>(
    "outbox-processor",
    x => x.ProcessAsync(CancellationToken.None),
    Cron.Minutely);

RecurringJob.AddOrUpdate<IInterviewReminderJob>(
    "interview-reminders",
    x => x.RunAsync(CancellationToken.None),
    Cron.Minutely);
await app.SeedRolesAsync();
await app.SeedSkillCatalogAsync();
app.Run();