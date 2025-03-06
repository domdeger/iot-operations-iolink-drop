using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTOperationsDrop.IOLink.Serialization;

public class TupleConverter<TKey, TValue> : JsonConverter<IEnumerable<(TKey, TValue)>>
{
    public TupleConverter(JsonSerializerOptions options) { }

    public override IEnumerable<(TKey, TValue)> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        IEnumerable<(TKey, TValue)> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        foreach (var (key, property) in value)
        {
            var keyName = key?.ToString() ?? throw new InvalidOperationException();

            var sanitizedKeyName = options.PropertyNamingPolicy?.ConvertName(keyName) ?? keyName;
            writer.WritePropertyName(sanitizedKeyName);

            var te = JsonSerializer.Serialize(property, options);
            writer.WriteRawValue(te);
        }

        writer.WriteEndObject();
    }
}
