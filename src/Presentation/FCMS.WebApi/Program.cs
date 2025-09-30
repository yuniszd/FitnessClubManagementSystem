using FCMS.Application.Abstracts;
using FCMS.Application.Abstracts.Repositories;
using FCMS.Application.Events;
using FCMS.Application.Validations.MemberValidations;
using FCMS.Domain.Entities;
using FCMS.Infrastructure.Messaging;
using FCMS.Infrastructure.Services;
using FCMS.Infrastructure.Settings;
using FCMS.Persistence.BackgroundJobs;
using FCMS.Persistence.Configurations;
using FCMS.Persistence.Contexts;
using FCMS.Persistence.Services;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------ Add Services ------------------
builder.Services.AddControllers()
    .AddFluentValidation(config =>
        config.RegisterValidatorsFromAssemblyContaining<CreateMemberDtoValidator>());

// ------------------ DbContext ------------------
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

// ------------------ Identity ------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<FitnessDbContext>()
.AddDefaultTokenProviders();

// ------------------ JWT ------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ------------------ Swagger ------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gym Management API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Authorization header using the Bearer scheme.",
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
});

// ------------------ Repositories ------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// ------------------ Application Services ------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ------------------ Background Jobs ------------------
builder.Services.AddTransient<SubscriptionReminderJob>();

// ------------------ RabbitMQ ------------------
// 1️⃣ Singleton Connection
builder.Services.AddSingleton(sp =>
{
    var host = builder.Configuration.GetValue<string>("RabbitMQ:Host");
    var factory = new ConnectionFactory
    {
        HostName = host,
        DispatchConsumersAsync = true
    };
    return factory.CreateConnection();
});

// 2️⃣ Channel Pool
builder.Services.AddSingleton<RabbitMqChannelPool>();

// 3️⃣ Publisher
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

// 4️⃣ Consumers with factory pattern
builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer<CustomerRegisteredEvent>>>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var channelPool = sp.GetRequiredService<RabbitMqChannelPool>();
    var queue = "customer_registered_queue";

    return new RabbitMqConsumer<CustomerRegisteredEvent>(channelPool, queue, scopeFactory, logger);
});

builder.Services.AddHostedService(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer<EmailMessage>>>();
    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
    var channelPool = sp.GetRequiredService<RabbitMqChannelPool>();
    var queue = builder.Configuration.GetValue<string>("RabbitMQ:QueueName");

    return new RabbitMqConsumer<EmailMessage>(channelPool, queue, scopeFactory, logger);
});



// ------------------ CORS ------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ------------------ Hangfire ------------------
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
          {
              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
              QueuePollInterval = TimeSpan.Zero,
              UseRecommendedIsolationLevel = true,
              DisableGlobalLocks = true
          }));
builder.Services.AddHangfireServer();

var app = builder.Build();

// ------------------ Automatic Role & Admin Creation ------------------
await CreateRolesAndAdminAsync(app);

// ------------------ Hangfire Recurring Job ------------------
await ConfigureRecurringJobsAsync(app);

// ------------------ Middleware ------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gym Management API V1"));
}

app.UseHangfireDashboard("/hangfire");
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// ================== Helper Methods ==================
async Task CreateRolesAndAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string[] roles = { "Admin", "Reception" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new AppUser { UserName = "admin", Email = "admin@example.com" };
        await userManager.CreateAsync(adminUser, "Admin123!");
        Console.WriteLine("Admin user yaradıldı: admin / Admin123!");
    }

    var userRoles = await userManager.GetRolesAsync(adminUser);
    if (!userRoles.Contains("Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        Console.WriteLine("Admin user-ə 'Admin' rolu əlavə olundu.");
    }
}

async Task ConfigureRecurringJobsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var subscriptionReminderJob = scope.ServiceProvider.GetRequiredService<SubscriptionReminderJob>();

    recurringJobManager.AddOrUpdate(
        "subscription-reminder-job",
        () => subscriptionReminderJob.SendRemindersAsync(),
        Cron.Daily
    );
}
