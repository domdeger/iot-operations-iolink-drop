using System.Text.Json;

namespace IoTOperationsDrop.IOLink.Serialization;

internal static class DefaultJsonSerializerSettings
{
    public static JsonSerializerOptions Settings { get; } =
        new JsonSerializerOptions { Converters = { new TupleConverterTKeyTValueConverter() } };
}
