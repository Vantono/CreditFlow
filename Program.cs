using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Base.Middleware;
using CreditFlowAPI.Base.Persistance;
using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Base.Hubs;
using CreditFlowAPI.Domain.Interfaces;
using CreditFlowAPI.Feature.Loans.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SETUP ��������� ---

builder.Services.AddControllers();

// A. NATIVE OPENAPI
builder.Services.AddOpenApi();

// B. CORS (with SignalR support)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreditFlowAPI.Feature.Loans.Commands.CreateLoanCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateLoanCommand>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=CreditFlow.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true
        };

        // Allow SignalR to use tokens from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// ��. User Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<RiskAssessmentService, RiskAssessmentService>();
builder.Services.AddScoped<LoanCalculationService, LoanCalculationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileService, FileService>();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {        
        options.SwaggerEndpoint("/openapi/v1.json", "CreditFlow API v1");
    });
}

app.UseHttpsRedirection();

// Serve static files from wwwroot/browser (Angular build output)
var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
    Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "browser"));
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = "" });

app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// SPA fallback - serve index.html for all non-API routes
app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = fileProvider });

await DbSeeder.SeedRolesAndBankerAsync(app);
app.Run();