using Newtonsoft.Json;
using Poltergeist.Common.Utilities.Cryptology;
using System.IO;

namespace Poltergeist.Common.Structures;

public abstract class JsonFile
{
    [JsonIgnore]
    private string Filepath
    {
        get; set;
    }

    protected JsonFile()
    {
    }

    protected JsonFile(string filepath)
    {
        Filepath = filepath;

        Load();
    }

    private void Load()
    {
        if (File.Exists(Filepath))
        {
            SerializationUtil.JsonPopulate(Filepath, this);
        }
    }

    public void Load(string filepath)
    {
        Filepath = filepath;

        Load();
    }

    public void Save()
    {
        SerializationUtil.JsonSave(Filepath, this);
    }

    public void Save(string filepath)
    {
        SerializationUtil.JsonSave(filepath, this);
    }
}
