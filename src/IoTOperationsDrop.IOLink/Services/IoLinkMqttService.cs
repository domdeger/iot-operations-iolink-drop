using System.Text.Json;
using IOLinkNET.Integration;
using IOLinkNET.Vendors.Ifm;
using IoTOperationsDrop.IOLink.Models.Settings;
using IoTOperationsDrop.IOLink.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using Spectre.Console;

namespace IoTOperationsDrop.IOLink.Services;

public class IoLinkMqttService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly TimeSpan _publishInterval;
    private readonly IODDPortReader _portReader;
    private readonly byte _ioLinkPort;

    public IoLinkMqttService(IConfiguration configuration, IOptions<MqttSettings> mqttSettings)
    {
        PrintWelcomeBanner();

        var settings = mqttSettings.Value;
        _publishInterval = TimeSpan.FromSeconds(settings.PublishIntervalSeconds);

        var mqttFactory = new MqttClientFactory();
        _client = mqttFactory.CreateMqttClient();

        var brokerHost = configuration["Mqtt:BrokerHost"] ?? Environment.GetEnvironmentVariable("MQTT__BROKERHOST");
        var brokerPort = Convert.ToInt32(configuration["Mqtt:BrokerPort"] ?? Environment.GetEnvironmentVariable("MQTT__BROKERPORT"));
        var username = configuration["Mqtt:Username"] ?? Environment.GetEnvironmentVariable("MQTT__USERNAME");
        var password = configuration["Mqtt:Password"] ?? Environment.GetEnvironmentVariable("MQTT__PASSWORD");
        var masterIp = configuration["IOLink:MasterIp"] ?? Environment.GetEnvironmentVariable("IOLink__MASTERIP");
        var ioLinkPort = configuration["IOLink:Port"] ?? Environment.GetEnvironmentVariable("IOÖINK__PORT");

        if (ioLinkPort is not null)
        {
            _ioLinkPort = byte.Parse(ioLinkPort);
        }
        else
        {
            throw new Exception("IOLink port is required");
        }


        var mqttOptions = new MqttClientOptionsBuilder()
            .WithClientId(settings.ClientId)
            .WithTcpServer(brokerHost, brokerPort)
            .WithCleanSession();

        if(username is not null && password is not null)
        {
            mqttOptions.WithCredentials(username, password);
        }

        _options = mqttOptions.Build();

        var masterConnection = IfmIoTCoreMasterConnectionFactory.Create($"http://{masterIp}");
        _portReader = PortReaderBuilder
            .NewPortReader()
            .WithMasterConnection(masterConnection)
            .WithConverterDefaults()
            .WithPublicIODDFinderApi()
            .Build();

        AnsiConsole.MarkupLine("[green]Configured IOLink MasterConnection and MQTT Client Options[/]"); 
        PrintConfiguration(brokerHost, brokerPort, username, password, masterIp, _ioLinkPort, settings.ClientId);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            AnsiConsole.MarkupLine("[yellow]# Execute MQTT Cycle[/]");

            if (!_client.IsConnected)
            {
                try
                {
                    await _client.ConnectAsync(_options, stoppingToken);
                    AnsiConsole.MarkupLine("[bold green]Successfully connected to MQTT Broker![/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[bold red]Failed to connect to MQTT Broker:[/] " + ex.Message);
                    return;
                }
            }

            await _portReader.InitializeForPortAsync(_ioLinkPort);

            while (!stoppingToken.IsCancellationRequested)
            {
                var processData = await _portReader.ReadConvertedProcessDataInAsync();

                const string topic = "iolink/pdin";

                var payload = new
                {
                    data = processData,
                    deviceId = _portReader.Device.ProfileBody.DeviceIdentity.DeviceId,
                    vendorId = _portReader.Device.ProfileBody.DeviceIdentity.VendorId
                    ,
                };
                var jsonPayload = JsonSerializer.Serialize(
                    payload,
                    DefaultJsonSerializerSettings.Settings
                );

                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(jsonPayload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _client.PublishAsync(mqttMessage, stoppingToken);

                AnsiConsole.MarkupLine($"[bold green]MQTT Message was published in Topic {topic}:[/] {jsonPayload}");

                await Task.Delay(_publishInterval, stoppingToken);
            }
        } catch(Exception ex)
        {
            AnsiConsole.Markup($"[red]Fehler:[/] {ex.Message}");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private void PrintWelcomeBanner()
    {
        var welcomeText = "AIO MQTT Sample";
        var asciiBanner = Figgle.FiggleFonts.SlantSmall.Render(welcomeText);

        AnsiConsole.MarkupLine("[bold green]Welcome to[/] [underline yellow]IoT Operations - IOLink MQTT Publish Sample[/]");
        AnsiConsole.Write(new Markup($"[bold green]{asciiBanner}[/]"));
    }

    private void PrintConfiguration(string brokerHost, int brokerPort, string? username, string? password, string masterIp, byte ioLinkPort, string clientId)
    {
        var table = new Table();

        table.AddColumn("Einstellung");
        table.AddColumn("Wert");

        table.AddRow("Broker Host", brokerHost);
        table.AddRow("Broker Port", brokerPort.ToString());
        table.AddRow("Username", username ?? "<empty>");
        table.AddRow("Password", password != null ? new('*', password.Length): "<empty>" );
        table.AddRow("IOLink Master IP", masterIp);
        table.AddRow("IOLink Port", ioLinkPort.ToString());
        table.AddRow("Client ID", clientId);
        table.AddRow("Publish Interval (Sekunden)", _publishInterval.TotalSeconds.ToString());

        AnsiConsole.MarkupLine("[bold yellow]Konfigurationseinstellungen:[/]");
        AnsiConsole.Write(table);
    }
}
