using System.Text;
using Newtonsoft.Json;

namespace Poltergeist.Automations.Structures.Parameters;

[JsonConverter(typeof(PasswordValueConverter))]
public class PasswordValue
{
    public string Value { get; set; }

    public PasswordValue()
    {
        Value = "";
    }

    public PasswordValue(string value)
    {
        Value = value;
    }

    public override string? ToString() => Value;

    public class PasswordValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }

            if (value is not PasswordValue pv)
            {
                throw new NotSupportedException();
            }

            var bytes = Encoding.UTF8.GetBytes(pv.Value);
            var base64 = Convert.ToBase64String(bytes);
            writer.WriteValue(base64);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string base64)
            {
                throw new NotSupportedException();
            }

            var bytes = Convert.FromBase64String(base64);
            var text = Encoding.UTF8.GetString(bytes);
            var pv = new PasswordValue(text);
            return pv;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PasswordValue);
        }
    }
}
