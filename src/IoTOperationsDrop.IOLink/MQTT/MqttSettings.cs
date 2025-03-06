namespace IoTOperationsDrop.IOLink.MQTT;

public class MqttSettings
{
    public string ClientId { get; set; }
    public string BrokerHost { get; set; }
    public int BrokerPort { get; set; }

    public int PublishIntervalSeconds { get; set; } = 30;
}
