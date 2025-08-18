using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Structures.Parameters;

[JsonConverter(typeof(PasswordValueConverter))]
public class PasswordValue
{
    public string Plaintext { get; set; }

    public PasswordValue()
    {
        Plaintext = "";
    }

    public PasswordValue(string plaintext)
    {
        Plaintext = plaintext;
    }

    public override string? ToString() => Plaintext;

    private class PasswordValueConverter : JsonConverter<PasswordValue>
    {
        public override void Write(Utf8JsonWriter writer, PasswordValue value, JsonSerializerOptions options)
        {
            var bytes = Encoding.UTF8.GetBytes(value.Plaintext);
            writer.WriteBase64StringValue(bytes);
        }

        public override PasswordValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var bytes = reader.GetBytesFromBase64();
            var plaintext = Encoding.UTF8.GetString(bytes);
            return new PasswordValue(plaintext);
        }
    }
}
