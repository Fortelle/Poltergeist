using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Storages;

public class LocalStorageService : FileStorageService
{
    public LocalStorageService(MacroProcessor processor) : base(processor, "private_folder")
    {
    }
}
