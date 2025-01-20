using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using UTAPI.Data;
using UTAPI.Interceptors;
using UTAPI.Interfaces;
using UTAPI.Middleware;
using UTAPI.Security;
using UTAPI.Services;
using UTAPI.Utils;
using UTAPI.Websocket;

var builder = WebApplication.CreateBuilder(args);

// Configure logging settings
builder.Logging.ClearProviders(); // Optional: Remove default providers (e.g., Console, Debug)
builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug();   // Add debug logging
builder.Logging.SetMinimumLevel(LogLevel.Information); // Set minimum log level

var tokenKey = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JwtSettings:Key n�o pode ser nulo"));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin() // Substitua pelo URL do seu frontend
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders("X-Count"));
});

builder.Services.AddControllers();
builder.Services.AddLogging();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IEntityServices, EntityServices>();
builder.Services.AddScoped<IEntityDriverServices, EntityDriverServices>();
builder.Services.AddScoped<IRegionServices, RegionServices>();
builder.Services.AddScoped<IRouteServices, RouteServices>();
builder.Services.AddScoped<IStopServices, StopServices>();
builder.Services.AddScoped<IRouteStopServices, RouteStopServices>();
builder.Services.AddScoped<IFavRouteServices, FavRouteServices>();
builder.Services.AddScoped<IPriceTableServices, PriceTableService>();
builder.Services.AddScoped<IPriceTableContentServices, PriceTableContentService>();
builder.Services.AddScoped<IStopServices, StopServices>();
builder.Services.AddScoped<IRouteLineServices, RouteLineServices>();
builder.Services.AddScoped<IRouteStopServices, RouteStopServices>();
builder.Services.AddScoped<IRouteHistoryServices, RouteHistoryServices>();
builder.Services.AddSingleton<TokenGenerator>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();
builder.Services.AddScoped<IAuditServices, AuditServices>();
builder.Services.AddScoped<ISessionServices, SessionServices>();
builder.Services.AddScoped<IDriverRouteServices, DriverRouteServices>();

builder.Services.AddDbContext<DataContext>((serviceProvider, opt) =>
{
    // Configure DbContext with Npgsql
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    x => x.MigrationsHistoryTable("__EFMigrationsHistory", builder.Configuration.GetSection("Schema").GetSection("DefaultSchema").Value));

    // Add the AuditSaveChangesInterceptor to the DbContext
    opt.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
});

builder.Services.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<InternalServerErrorExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
{
    x.MapInboundClaims = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Usar CORS
app.UseCors("AllowAllOrigins");

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(10) // Optional, default: 2 mins
};

app.UseWebSockets(webSocketOptions);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var roomManager = context.RequestServices.GetRequiredService<RoomManager>();
        roomManager.StartInactiveRoomCleanup();

        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        // Get the logger from DI container
        var logger = context.RequestServices.GetRequiredService<ILogger<WebSocketHandler>>();

        var middleware = new WebSocketMiddleware(next: _ => Task.CompletedTask, tokenValidationParameters, roomManager, logger);
        await middleware.InvokeAsync(context);
    }
    else
    {
        context.Response.StatusCode = 400; // Bad Request
    }
});

app.Run();
