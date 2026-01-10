using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Base.Middleware;
using CreditFlowAPI.Base.Persistance;
using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Domain.Interfaces;
using CreditFlowAPI.Feature.Loans.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SETUP ΥΠΗΡΕΣΙΩΝ ---

builder.Services.AddControllers();

// A. NATIVE OPENAPI
builder.Services.AddOpenApi();

// B. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Γ. MediatR & Validators
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreditFlowAPI.Feature.Loans.Commands.CreateLoanCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateLoanCommand>();

// Δ. Database & Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=CreditFlow.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Ε. Authentication (JWT)
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
    });

// ΣΤ. User Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<TokenService, TokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
var app = builder.Build();
// Προσθήκη Middleware για Global Exception Handling
app.UseMiddleware<ExceptionMiddleware>();
// --- 2. SETUP PIPELINE ---

if (app.Environment.IsDevelopment())
{
    // 1. Δημιουργία JSON Endpoint (Native)
    app.MapOpenApi();

    // 2. Ενεργοποίηση Swagger UI (Διαβάζει το native json)
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CreditFlow API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
await DbSeeder.SeedRolesAndBankerAsync(app);
app.Run();