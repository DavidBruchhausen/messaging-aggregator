using MessagingAggregator.Api.Middleware;
using MessagingAggregator.Application;
using MessagingAggregator.Gateway;
using MessagingAggregator.HostedService;
using Serilog;

internal class Program
{
    private static readonly Dictionary<string, string> _details = new();
    private static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting up");
        var builder = WebApplication.CreateBuilder(args);
        _details.Add("Environment", builder.Environment.EnvironmentName ?? "Local");

        // Add services to the container.
        builder.Services.AddControllers();
        ConfigureSettings(builder.Configuration);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddHostedService();
        builder.Services.AddGateway();

        var app = builder.Build();

        EnableMiddleware(app);

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    static void ConfigureSettings(ConfigurationManager configuration)
    {
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{_details["Environment"]}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    static void EnableMiddleware(IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }

    static void ConfigureLogging(IHostBuilder host)
    {
        host.UseSerilog((ctx, lc) => lc
            .WriteTo.Console()
            .ReadFrom.Configuration(ctx.Configuration));
    }
}