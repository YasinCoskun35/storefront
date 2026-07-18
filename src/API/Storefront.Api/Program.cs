using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Storefront.Api.Extensions;
using Storefront.Api.Middleware;
using Storefront.Modules.Catalog;
using Storefront.Modules.Content;
using Storefront.Modules.Identity;
using Storefront.Modules.Orders;

// ── Bootstrap logger — captures startup failures before full Serilog is configured ──
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Storefront API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog (reads full config from appsettings.json "Serilog" section) ──────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // ── Validate required secrets at startup — fail fast rather than at first request ─
    var jwtSecret      = builder.Configuration["Jwt:Secret"];
    var connectionStr  = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(jwtSecret))
        throw new InvalidOperationException(
            "Jwt:Secret is not configured. Set it via user-secrets (dev) or environment variable Jwt__Secret (prod).");

    if (string.IsNullOrWhiteSpace(connectionStr))
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured. Set it via user-secrets or ConnectionStrings__DefaultConnection.");

    // ── MVC + JSON ────────────────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();

    // ── CORS — origins read from config, never hardcoded ─────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
        options.AddPolicy("AllowFrontend", policy =>
        {
            if (allowedOrigins.Length == 0)
            {
                Log.Warning("Cors:AllowedOrigins is empty — CORS is disabled. " +
                            "Set origins via Cors__AllowedOrigins__0, etc.");
                // Reject all cross-origin requests when no origins are configured
                policy.SetIsOriginAllowed(_ => false);
            }
            else
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // required for httpOnly cookies
            }
        }));

    // ── Rate Limiting (built-in, .NET 7+) ────────────────────────────────────────────
    var authWindow      = builder.Configuration.GetValue<int>("RateLimit:AuthWindowSeconds", 60);
    var authPermitLimit = builder.Configuration.GetValue<int>("RateLimit:AuthPermitLimit", 10);

    builder.Services.AddRateLimiter(options =>
    {
        // Applied to all /auth/login and /partners/auth/login endpoints via [EnableRateLimiting]
        options.AddSlidingWindowLimiter("AuthPolicy", opt =>
        {
            opt.PermitLimit           = authPermitLimit;
            opt.Window                = TimeSpan.FromSeconds(authWindow);
            opt.SegmentsPerWindow     = 6; // 6 segments → 10-second precision
            opt.QueueProcessingOrder  = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit            = 0; // reject immediately when full
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)authWindow).ToString(System.Globalization.CultureInfo.InvariantCulture);
            await context.HttpContext.Response.WriteAsJsonAsync(
                new { error = "RateLimit.Exceeded", message = "Too many requests. Please try again later." },
                cancellationToken: token);
        };
    });

    // ── Swagger ───────────────────────────────────────────────────────────────────────
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title       = "Storefront API",
            Version     = "v1",
            Description = "B2B Furniture Platform — Modular Monolith Architecture"
        });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT via Authorization header. Example: 'Bearer {token}'",
            Name        = "Authorization",
            In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme      = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── Domain modules ────────────────────────────────────────────────────────────────
    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddContentModule(builder.Configuration);
    builder.Services.AddOrdersModule(builder.Configuration);

    // ── Health checks ─────────────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionStr!,                       // already validated above — never null here
            name:    "postgres",
            tags:    ["db", "ready"],
            timeout: TimeSpan.FromSeconds(5));

    var app = builder.Build();

    // ── Database init & seed ──────────────────────────────────────────────────────────
    await app.Services.InitializeDatabasesAsync();
    await app.Services.SeedIdentityDataAsync();

    // ────────────────────────────────────────────────────────────────────────────────────
    // MIDDLEWARE PIPELINE — order is critical
    // 1. Global error handler  (outermost — catches everything below)
    // 2. HSTS / HTTPS redirect (transport security)
    // 3. Serilog request logging
    // 4. Swagger (dev only)
    // 5. CORS                  (before auth — sets headers on all requests incl. preflight)
    // 6. Rate limiting
    // 7. Static files          (no auth needed for uploaded images)
    // 8. Authentication
    // 9. Authorization
    // 10. Controllers
    // ────────────────────────────────────────────────────────────────────────────────────

    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        // HSTS: tell browsers to only use HTTPS for 1 year (preload-ready)
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // Structured HTTP request/response logging via Serilog
    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost",   httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent",     httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Storefront API v1");
            options.RoutePrefix      = "swagger";
            options.DocumentTitle    = "Storefront API Documentation";
            options.DefaultModelsExpandDepth(-1);
        });
    }

    app.UseCors("AllowFrontend");
    app.UseRateLimiter();

    // Serve uploaded product images — no auth required
    var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    Directory.CreateDirectory(uploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath     = "/uploads",
        ServeUnknownFileTypes = false, // only serve known types (jpg, png, webp, etc.)
        OnPrepareResponse = ctx =>
        {
            // Prevent caching of images in CDN to allow updates
            ctx.Context.Response.Headers.CacheControl = "public, max-age=3600";
        }
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // /health/ready — checks DB connectivity (used by load balancers / orchestrators)
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("ready")
    });

    // /health/live — always 200 while the process is running (liveness probe)
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false   // no checks — just confirms the process is up
    });

    // /health — plain alias used by the Docker/nginx healthchecks and deploy docs
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    Log.Information("Storefront API started successfully");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
