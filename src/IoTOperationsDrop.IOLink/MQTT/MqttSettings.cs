namespace IoTOperationsDrop.IOLink.MQTT;

public class MqttSettings
{
    public required string ClientId { get; set; }
    public required string BrokerHost { get; set; }
    public int BrokerPort { get; set; }

    public int PublishIntervalSeconds { get; set; } = 30;
}
