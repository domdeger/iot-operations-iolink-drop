using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTOperationsDrop.IOLink.Serialization;

public class TupleConverterTKeyTValueConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        var genericTypeDef = typeToConvert.GetGenericTypeDefinition();

        var enumerableType = typeof(IEnumerable);
        var isEnumerable = typeToConvert.IsAssignableTo(enumerableType);

        var innerType = typeToConvert.GetGenericArguments().FirstOrDefault();

        if (
            isEnumerable
            && innerType?.IsGenericType == true
            && innerType.GetGenericTypeDefinition() == typeof(ValueTuple<,>)
        )
        {
            return true;
        }

        return false;
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var genericTypeDef = type.GetGenericArguments().First().GetGenericArguments();

        Type keyType = genericTypeDef[0];
        Type valueType = genericTypeDef[1];

        JsonConverter converter = (JsonConverter)
            Activator.CreateInstance(
                typeof(TupleConverter<,>).MakeGenericType([keyType, valueType]),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [options],
                culture: null
            )!;

        return converter;
    }
}
