using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Buddy.Api.Middleware;
using Buddy.Application.Common.Interfaces;
using Buddy.Application.Features.Auth.Register;
using Buddy.Application.Services;
using Buddy.Infrastructure.Services;
using Buddy.Infrastructure.Services.ElevenLabs;
using Buddy.Infrastructure.Services.Gemini;
using Buddy.Persistence;
using Buddy.Persistence.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddPersistenceServices();

// 2. MediatR
object value = builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
});
// 3. Dependency Injection
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<IInterviewLLMService>(sp => sp.GetRequiredService<GeminiService>());
builder.Services.AddScoped<IQuizLLMService>(sp => sp.GetRequiredService<GeminiService>());
builder.Services.AddScoped<ILLMService>(sp => sp.GetRequiredService<GeminiService>());
builder.Services.AddHttpClient<ITextToSpeechService, ElevenLabsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddValidatorsFromAssembly(typeof(Buddy.Application.Features.Auth.Register.RegisterCommand).Assembly);

// 3.1 Redis & Caching
var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => 
    StackExchange.Redis.ConnectionMultiplexer.Connect(redisConn));

builder.Services.AddSingleton<ICacheService>(sp => 
    new Buddy.Infrastructure.Services.Redis.RedisCacheService(
        sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>(),
        "buddy:",
        3600 // Default TTL 1 hour
    ));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGlobalCache>(sp =>
    new Buddy.Infrastructure.Services.Redis.GlobalCache(
        sp.GetRequiredService<ICacheService>(),
        sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>(),
        "buddy:",
        sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()
    ));

// 4. JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_1234567890123456";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "StudyBuddy",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "StudyBuddyUser",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 6. Enhanced Swagger with Auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "StudyBuddy API - AI Interview Platform", 
        Version = "v1.0",
        Description = "REST API for AI-powered interview practice platform with speech recognition and feedback",
        Contact = new OpenApiContact
        {
            Name = "StudyBuddy Team",
            Email = "support@studybuddy.com"
        }
    });

    // XML Documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Security
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // Group by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});
builder.Services.AddHttpContextAccessor(); 
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyBuddy API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "StudyBuddy API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(0);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
