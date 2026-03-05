using Freeqy_APIs.Configrautions;
using Freeqy_APIs.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Refit;

namespace Freeqy_APIs;

public static class Dependenceies
{
    public static IServiceCollection AddDependency(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            )
        );

        services.AddAuthConfig(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        // Add swagger with proper configuration
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Freeqy Platform API",
                Version = "v1",
                Description = "Backend API for team formation and project management"
            });
            
            // Add JWT Authentication to Swagger
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments (for better documentation) and other helpful settings
            var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.SupportNonNullableReferenceTypes();
            options.CustomSchemaIds(type => type.FullName);
        });

        //Add Mapster
        services.AddMapsterDependcy();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
        services.AddScoped<IAiAnalysisService, AiAnalysisService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddFluentValidation();
        
        services.AddHttpContextAccessor();
        
        // Configure Mail
        services.Configure<MailConfig>(configuration.GetSection(nameof(MailConfig)));

        // Configure OAuth Providers
        services.AddOAuth(configuration);

        // Add Rate Limiting
        services.AddRateLimitingConfig(configuration);

        // Add Refit service to make sure AI service is Working 
        services.AddAIServices();

        // Add SignalR for real-time messaging
        services.AddSignalR();

        return services;
    }

    private static IServiceCollection AddOAuth(this IServiceCollection service, IConfiguration configuration)
    {
        service.Configure<GoogleOAuthOptions>(configuration.GetSection(GoogleOAuthOptions.SectionName));
        service.Configure<GitHubOAuthOptions>(configuration.GetSection(GitHubOAuthOptions.SectionName));
        
        
        
        return service;
    }

    private static IServiceCollection AddAIServices(this IServiceCollection services)
    {

        services.AddRefitClient<IAiServices>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("http://127.0.0.1:8000/api/v1");
            });

        return services;
    }

    private static IServiceCollection AddMapsterDependcy(this IServiceCollection services)
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));
        return  services;
    }

    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        return services;
    }

    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations();

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        var googleConfig = configuration.GetSection(GoogleOAuthOptions.SectionName).Get<GoogleOAuthOptions>();
        var githubConfig = configuration.GetSection(GitHubOAuthOptions.SectionName).Get<GitHubOAuthOptions>();

        var authBuilder = services.AddAuthentication()
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience
            };
        });

        // Add Google OAuth only if configured
        if (!string.IsNullOrWhiteSpace(googleConfig?.ClientId) && !string.IsNullOrWhiteSpace(googleConfig?.ClientSecret))
        {
            authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleConfig.ClientId;
                options.ClientSecret = googleConfig.ClientSecret;
                options.CallbackPath = googleConfig.RedirectUri;
                options.SaveTokens = true;
                foreach (var scope in googleConfig.Scopes)
                {
                    options.Scope.Add(scope);
                }
            });
        }

        // Add GitHub OAuth only if configured
        if (!string.IsNullOrWhiteSpace(githubConfig?.ClientId) && !string.IsNullOrWhiteSpace(githubConfig?.ClientSecret))
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = githubConfig.ClientId;
                options.ClientSecret = githubConfig.ClientSecret;
                options.CallbackPath = "/signin-github";
                options.SaveTokens = true;
                foreach (var scope in githubConfig.Scopes)
                {
                    options.Scope.Add(scope);
                }
            });
        }

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }

    private static IServiceCollection AddRateLimitingConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() 
            ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global Rate Limiter - applies to all endpoints
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var userId = context.User.Identity?.IsAuthenticated == true
                    ? context.User.GetUserId()
                    : context.Connection.RemoteIpAddress?.ToString();

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Global.Window),
                        QueueLimit = rateLimitOptions.Global.QueueLimit
                    });
            });

            // Authentication Rate Limiter - for login/register endpoints
            options.AddPolicy("authentication", context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Authentication.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Authentication.Window),
                        QueueLimit = rateLimitOptions.Authentication.QueueLimit
                    });
            });

            // API Rate Limiter - for general API endpoints
            options.AddPolicy("api", context =>
            {
                var userId = context.User.Identity?.IsAuthenticated == true
                    ? context.User.GetUserId()
                    : context.Connection.RemoteIpAddress?.ToString();

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Api.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Api.Window),
                        QueueLimit = rateLimitOptions.Api.QueueLimit
                    });
            });

            // Messaging Rate Limiter - stricter limits for chat messages
            options.AddPolicy("messaging", context =>
            {
                var userId = context.User.Identity?.IsAuthenticated == true
                    ? context.User.GetUserId()
                    : context.Connection.RemoteIpAddress?.ToString();

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Messaging.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Messaging.Window),
                        QueueLimit = rateLimitOptions.Messaging.QueueLimit
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                TimeSpan? retryAfter = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue))
                {
                    retryAfter = retryAfterValue;
                    context.HttpContext.Response.Headers.RetryAfter = retryAfterValue.TotalSeconds.ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var error = new
                {
                    type = "RateLimit.TooManyRequests",
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Rate limit exceeded. Please try again later.",
                    retryAfter = retryAfter?.TotalSeconds
                };

                await context.HttpContext.Response.WriteAsJsonAsync(error, cancellationToken);
            };
        });

        return services;
    }
}