using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Modules.Macros;

public class ProcessorReportCollection : IEnumerable<ProcessorReport>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        IncludeFields = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    private readonly List<ProcessorReport> Reports = new();

    public string? Filepath { get; set; }

    public void Load(string filepath)
    {
        Filepath = filepath;

        if (!File.Exists(Filepath))
        {
            return;
        }

        try
        {
            var text = File.ReadAllText(Filepath);
            var array = JsonSerializer.Deserialize<Dictionary<string, JsonNode>[]>(text, SerializerOptions);
            if (array is null)
            {
                return;
            }
            foreach (var dict in array)
            {
                var report = new ProcessorReport();
                foreach (var (key, jsonNode) in dict)
                {
                    if (jsonNode is not null)
                    {
                        report.Add(key, jsonNode);
                    }
                }
                Reports.Add(report);
            }

        }
        catch
        {
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(Filepath))
        {
            return;
        }

        var text = JsonSerializer.Serialize(Reports, SerializerOptions);

        var folder = Path.GetDirectoryName(Filepath);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(Filepath, text);
    }

    public void Add(ProcessorReport report)
    {
        Reports.Add(report);
    }

    public IEnumerator<ProcessorReport> GetEnumerator() => Reports.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
