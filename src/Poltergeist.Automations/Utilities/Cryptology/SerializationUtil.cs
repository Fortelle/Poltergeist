using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Utilities.Cryptology;

public static class SerializationUtil
{
    public static string JsonStringify<T>(T obj, Type[]? types = null)
    {
        var serializer = CreateSerializer(types);

        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new JsonTextWriter(sw);

        serializer.Serialize(writer, obj);

        return sb.ToString();
    }

    public static void JsonSave<T>(string path, T obj, Type[]? types = null)
    {
        var text = JsonStringify(obj, types);

        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var tempPath = path + ".temp";
        File.WriteAllText(tempPath, text);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.Move(tempPath, path);
    }

    private static JsonSerializer CreateSerializer(Type[]? types)
    {
        var serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
            //ContractResolver = CustomContractResolver.Instance,
        };

        serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        serializer.Converters.Add(new BitmapConverter());
        serializer.Converters.Add(new BitArrayConverter());

        if (types?.Length > 0)
        {
            var list = AssemblyUtil.GetSubclasses(types).ToList();
            serializer.SerializationBinder = new TypeBinder(list);
        }

        return serializer;
    }

    public static void JsonLoad<T>(string path, out T? obj, Type[]? types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        obj = serializer.Deserialize<T>(reader);
    }

    public static void JsonDeserialize<T>(string text, out T? obj, Type[]? types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StringReader(text);
        using var reader = new JsonTextReader(sr);

        obj = serializer.Deserialize<T>(reader);
    }

    public static void JsonPopulate(string path, object obj, Type[]? types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        serializer.Populate(reader, obj);
    }

    public static T JsonClone<T>(T obj) where T : notnull
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        var json = JsonConvert.SerializeObject(obj, settings);
        var clone = JsonConvert.DeserializeObject<T>(json, settings);

        return clone!;
    }

    public class TypeBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public TypeBinder(IList<Type> knownTypes)
        {
            KnownTypes = knownTypes;
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            return KnownTypes.Single(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }

    public static T? ToObjectAsync<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }

    public static string StringifyAsync(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public class BitmapConverter : JsonConverter<Bitmap>
    {
        public override void WriteJson(JsonWriter writer, Bitmap? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }
            using var ms = new MemoryStream();
            value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var buffer = ms.ToArray();
            var base64 = Convert.ToBase64String(buffer);
            writer.WriteValue(base64);
        }

        public override Bitmap? ReadJson(JsonReader reader, Type objectType, Bitmap? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string base64)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            var buffer = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(buffer);
            return new Bitmap(ms);
        }
    }

    public class BitArrayConverter : JsonConverter<BitArray>
    {
        public override void WriteJson(JsonWriter writer, BitArray? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }

            var bytes = new byte[(value.Length - 1) / 8 + 1];
            value.CopyTo(bytes, 0);
            var hash = string.Concat(bytes.Select(x => x.ToString("X2")));
            writer.WriteValue(hash);
        }

        public override BitArray? ReadJson(JsonReader reader, Type objectType, BitArray? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string base64)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            var bytes = Regex.Matches(base64, "..")
                .Cast<Match>()
                .Select(x => Convert.ToByte(x.Value, 16))
                .ToArray();
            var bits = new BitArray(bytes);
            return bits;
        }
    }

    public class CustomContractResolver : DefaultContractResolver
    {
        public static readonly CustomContractResolver Instance = new();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable && member.GetCustomAttribute<JsonPropertyAttribute>() is null)
            {
                property.Ignored = true;
            }

            return property;
        }
    }

}
