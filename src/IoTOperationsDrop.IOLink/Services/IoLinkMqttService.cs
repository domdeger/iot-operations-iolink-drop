using System.Text.Json;
using IOLinkNET.Integration;
using IOLinkNET.Vendors.Ifm;
using IoTOperationsDrop.IOLink.MQTT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace IoTOperationsDrop.IOLink.Services;

public class IoLinkMqttService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly TimeSpan _publishInterval;
    private readonly IODDPortReader _portReader;
    
    public IoLinkMqttService(IConfiguration configuration, IOptions<MqttSettings> mqttSettings)
    {
        var settings = mqttSettings.Value;            
        _publishInterval = TimeSpan.FromSeconds(settings.PublishIntervalSeconds);

        var mqttFactory = new MqttClientFactory();
        _client = mqttFactory.CreateMqttClient();            

        var username = configuration["Mqtt:Username"];
        var password = configuration["Mqtt:Password"];

        _options = new MqttClientOptionsBuilder()
            .WithClientId(settings.ClientId)
            .WithTcpServer(settings.BrokerHost, settings.BrokerPort)
            .WithCredentials(username, password)
            .WithCleanSession()
            .Build();
        
        
        var masterConnection = IfmIoTCoreMasterConnectionFactory.Create("http://192.168.0.113");
        _portReader = PortReaderBuilder.NewPortReader()
            .WithMasterConnection(masterConnection)
            .WithConverterDefaults()
            .WithPublicIODDFinderApi()
            .Build();
        _portReader.InitializeForPortAsync(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.ConnectAsync(_options, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processData = await _portReader.ReadConvertedProcessDataInAsync();
            
            const string topic = "iolink/pdin";

            var payload = new
            {
                data = processData, 
                deviceId = "TODO Read Device ID",//_portReader.Device.ProfileBody.DeviceIdentity.DeviceId,
                vendorId = "TODO Read Vendor ID"//_portReader.Device.ProfileBody.DeviceIdentity.VendorId
            };
            var jsonPayload = JsonSerializer.Serialize(payload);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(jsonPayload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await _client.PublishAsync(mqttMessage, stoppingToken);
            await Task.Delay(_publishInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.DisconnectAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}