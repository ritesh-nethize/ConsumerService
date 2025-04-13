using ConsumerService.Data;
using ConsumerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

try
{
    Log.Information("Starting ConsumerService...");

    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.AddDbContext<PostDbContext>(options =>
                options.UseSqlServer("Server=localhost;Database=PostDb;Trusted_Connection=True;TrustServerCertificate=True;"));

            services.AddHostedService<PostConsumerWorker>();
        });

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();


}