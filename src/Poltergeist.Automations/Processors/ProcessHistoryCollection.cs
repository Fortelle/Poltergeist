using Poltergeist.Automations.Utilities.Cryptology;

namespace Poltergeist.Automations.Processors;

public class ProcessHistoryCollection
{
    private readonly List<ProcessHistoryEntry> Entries = new();

    private string? Filepath;

    public void Load(string filepath)
    {
        Filepath = filepath;

        if (!File.Exists(Filepath))
        {
            return;
        }

        SerializationUtil.JsonPopulate(Filepath, Entries);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(Filepath))
        {
            return;
        }

        SerializationUtil.JsonSave(Filepath, Entries);
    }

    public void Add(ProcessHistoryEntry history)
    {
        Entries.Add(history);
    }

    public ProcessHistoryEntry[] Take(int count)
    {
        return Entries
            .OrderByDescending(x => x.StartTime)
            .Take(count)
            .ToArray();
    }
}
