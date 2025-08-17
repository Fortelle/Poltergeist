using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Storages;

public class GlobalStorageService : FileStorageService
{
    public GlobalStorageService(MacroProcessor processor) : base(processor, "document_data_folder")
    {
    }
}
