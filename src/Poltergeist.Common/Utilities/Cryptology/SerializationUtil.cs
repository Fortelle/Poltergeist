using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Poltergeist.Common.Utilities.Cryptology;

public static class SerializationUtil
{
    public static void XmlSerialize<T>(string path, T obj, params Type[] types)
    {
        var folder = Path.GetDirectoryName(path);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var tempPath = path + ".temp";
        var xs = new XmlSerializer(obj.GetType(), types);
        using (var sw = new StreamWriter(tempPath))
        {
            xs.Serialize(sw, obj);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.Move(tempPath, path);
    }

    public static void XmlDeserialize<T>(string path, out T obj, params Type[] types)
    {
        var xs = new XmlSerializer(typeof(T), types);
        using var sr = new StreamReader(path);
        obj = (T)xs.Deserialize(sr);
    }

    public static string JsonStringify<T>(T obj, Type[] types = null)
    {
        var serializer = CreateSerializer(types);

        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new JsonTextWriter(sw);

        serializer.Serialize(writer, obj);

        return sb.ToString();
    }

    public static void JsonSave<T>(string path, T obj, Type[] types = null)
    {
        var text = JsonStringify(obj, types);

        var folder = Path.GetDirectoryName(path);
        if (!Directory.Exists(folder))
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

    private static JsonSerializer CreateSerializer(Type[] types)
    {
        var serializer = new JsonSerializer
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
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
            serializer.SerializationBinder = new TypeBinder
            {
                KnownTypes = AssemblyUtil.GetSubclasses(types).ToList(),
            };
        }

        return serializer;
    }

    public static void JsonLoad<T>(string path, out T obj, Type[] types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        obj = serializer.Deserialize<T>(reader);
    }

    public static void JsonDeserialize<T>(string text, out T obj, Type[] types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StringReader(text);
        using var reader = new JsonTextReader(sr);

        obj = serializer.Deserialize<T>(reader);
    }

    public static void JsonPopulate(string path, object obj, Type[] types = null)
    {
        var serializer = CreateSerializer(types);

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        serializer.Populate(reader, obj);
    }

    public static T JsonClone<T>(T obj)
    {
        var set = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Auto,
        };
        set.Converters.Add(new BitmapConverter());

        var json = JsonConvert.SerializeObject(obj, set);
        var clone = JsonConvert.DeserializeObject(json, obj.GetType(), set);

        return (T)clone;
    }

    public class TypeBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes
        {
            get; set;
        }

        public TypeBinder()
        {
        }

        public TypeBinder(IList<Type> knownTypes)
        {
            KnownTypes = knownTypes;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }

    public static T ToObjectAsync<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }

    public static string StringifyAsync(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public class BitmapConverter : JsonConverter<Bitmap>
    {
        public override void WriteJson(JsonWriter writer, Bitmap value, JsonSerializer serializer)
        {
            //if (value != null)
            //{
            using var ms = new MemoryStream();
            value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var buffer = ms.ToArray();
            var base64 = Convert.ToBase64String(buffer);
            writer.WriteValue(base64);
            //}
        }

        public override Bitmap ReadJson(JsonReader reader, Type objectType, Bitmap existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var base64 = (string)reader.Value;
            if (string.IsNullOrEmpty(base64))
            {
                return null;
            }
            else
            {
                var buffer = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(buffer);
                return new Bitmap(ms);
            }
        }
    }

    public class BitArrayConverter : JsonConverter<BitArray>
    {
        public override void WriteJson(JsonWriter writer, BitArray value, JsonSerializer serializer)
        {
            //if (value != null)
            //{
            var bytes = new byte[(value.Length - 1) / 8 + 1];
            value.CopyTo(bytes, 0);
            var hash = string.Concat(bytes.Select(x => x.ToString("X2")));
            writer.WriteValue(hash);
            //}
        }

        public override BitArray ReadJson(JsonReader reader, Type objectType, BitArray existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var base64 = (string)reader.Value;
            if (string.IsNullOrEmpty(base64))
            {
                return null;
            }
            else
            {
                var bytes = Regex.Matches(base64, "..")
                    .Cast<Match>()
                    .Select(x => Convert.ToByte(x.Value, 16))
                    .ToArray();
                var bits = new BitArray(bytes);
                return bits;
            }
        }
    }

    public class CustomContractResolver : DefaultContractResolver
    {
        public static readonly CustomContractResolver Instance = new CustomContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable && member.GetCustomAttribute<JsonPropertyAttribute>() == null)
                property.Ignored = true;

            return property;
        }
    }

}
