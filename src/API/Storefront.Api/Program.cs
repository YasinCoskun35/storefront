using Storefront.Api.Extensions;
using Storefront.Modules.Identity;
using Storefront.Modules.Catalog;
using Storefront.Modules.Content;
using Storefront.Modules.Orders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow string enums in JSON (e.g., "InStock" instead of 1)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow all origins for easier testing
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // In production, use strict origin checking
            policy.WithOrigins(
                    "http://localhost:3000", 
                    "http://localhost:3001",
                    "https://localhost:3000",
                    "https://localhost:3001"
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Storefront API",
        Version = "v1",
        Description = "Hardware Store Product Catalog API - Modular Monolith Architecture",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Storefront Team",
            Email = "admin@storefront.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
});

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddContentModule(builder.Configuration);
builder.Services.AddOrdersModule(builder.Configuration);

var app = builder.Build();

// Initialize databases
await app.Services.InitializeDatabasesAsync();

// Seed Identity data
await app.Services.SeedIdentityDataAsync();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Storefront API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Storefront API Documentation";
        options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    });
}

// Enable CORS (MUST be before Authentication/Authorization)
app.UseCors("AllowFrontend");

// Disable HTTPS redirection in development (causes certificate errors)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve static files from uploads directory
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath); // Ensure directory exists

Console.WriteLine($"📁 Configuring static files from: {uploadsPath}");
Console.WriteLine($"📁 Directory exists: {Directory.Exists(uploadsPath)}");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx =>
    {
        Console.WriteLine($"📸 Serving file: {ctx.File.Name}");
    }
});

Console.WriteLine("✅ Static files middleware configured");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
