using IoTOperationsDrop.IOLink.Models.Settings;
using IoTOperationsDrop.IOLink.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (hostingContext, config) =>
        {
            config.AddUserSecrets<Program>();
            config.AddEnvironmentVariables();
        }
    );
builder.ConfigureServices(
    (hostingContext, services) =>
    {
        services.AddOptions();
        services.Configure<MqttSettings>(hostingContext.Configuration.GetSection("Mqtt"));
        services.Configure<IoLinkSettings>(hostingContext.Configuration.GetSection("IOLink"));
        services.AddHostedService<IoLinkMqttService>();
    }
);

var host = builder.Build();

await host.RunAsync();
