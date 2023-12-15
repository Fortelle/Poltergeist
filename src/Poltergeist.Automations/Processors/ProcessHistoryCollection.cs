using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Processors;

public class ProcessHistoryCollection
{
    private List<ProcessHistoryEntry> Entries { get; } = new();

    private string Filepath { get; set; }

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
        SerializationUtil.JsonSave(Filepath, Entries);
    }

    public void Add(ProcessHistoryEntry history)
    {
        Entries.Add(history);
        Save();
    }

    public ProcessHistoryEntry[] Take(int count)
    {
        return Entries
            .OrderByDescending(x => x.StartTime)
            .Take(count)
            .ToArray();
    }
}
