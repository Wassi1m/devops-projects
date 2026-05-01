using System.Text;
using BourseIA.Data;
using BourseIA.Hubs;
using BourseIA.Services;
using BourseIA.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// DATABASE + LOGS
// ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
);

// ─────────────────────────────────────────────
// JWT AUTH
// ─────────────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secret = jwtConfig["Cle"]
    ?? throw new InvalidOperationException("Jwt:Cle manquante");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtConfig["Emetteur"],

        ValidateAudience = true,
        ValidAudience = jwtConfig["Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────
// SIGNALR
// ─────────────────────────────────────────────
builder.Services.AddSignalR();

// ─────────────────────────────────────────────
// CORS
// ─────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("BourseIAPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000" }
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// ─────────────────────────────────────────────
// HTTP CLIENT
// ─────────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("AIClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ─────────────────────────────────────────────
// SERVICES
// ─────────────────────────────────────────────
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IIdeaService, IdeaService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IConfidenceAnalysisService, ConfidenceAnalysisService>();

// ─────────────────────────────────────────────
// CONTROLLERS
// ─────────────────────────────────────────────
builder.Services.AddControllers();

// ─────────────────────────────────────────────
// SWAGGER / OPENAPI
// ─────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─────────────────────────────────────────────
// MIDDLEWARE
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference(options =>
    {
        options.Title = "BourseIA API";
        options.Theme = ScalarTheme.DeepSpace;
    });
}


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("BourseIAPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// ─────────────────────────────────────────────
// AUTO MIGRATION + DEBUG + RETRY
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    int retries = 5;

    while (retries > 0)
    {
        try
        {
            Console.WriteLine("🔄 Tentative de connexion à la DB...");
            db.Database.Migrate();
            Console.WriteLine("✅ Migration réussie !");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ ERREUR DB:");
            Console.WriteLine($"Message: {ex.Message}");

            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");

            retries--;

            if (retries == 0)
            {
                Console.WriteLine("🚨 ECHEC TOTAL DB !");
                throw;
            }

            Console.WriteLine("⏳ Nouvelle tentative dans 5 secondes...");
            Thread.Sleep(5000);
        }
    }
}

app.Run();
