namespace IoTOperationsDrop.IOLink.Models.Settings;

public class MqttSettings
{
    public required string ClientId { get; set; }
    public int PublishIntervalSeconds { get; set; } = 30;
}
