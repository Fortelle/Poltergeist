using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.Helpers;

public static class InteractionHelper
{
    public static async Task Interact(NotificationModel model)
    {
        switch (model)
        {
            case TipModel tipModel:
                TipService.Show(tipModel);
                break;
            case DialogModel dialogModel:
                await DialogService.ShowAsync(dialogModel);
                break;
            case ToastModel toastModel:
                PoltergeistApplication.GetService<AppNotificationService>().Show(toastModel);
                break;
            case FileOpenModel fileOpenModel:
                await DialogService.ShowFileOpenPickerAsync(fileOpenModel);
                break;
            case FileSaveModel fileSaveModel:
                await DialogService.ShowFileSavePickerAsync(fileSaveModel);
                break;
            case FolderModel folderModel:
                await DialogService.ShowFolderPickerAsync(folderModel);
                break;
            case NavigationModel navigationModel:
                App.TryEnqueue(() =>
                {
                    PoltergeistApplication.GetService<NavigationService>().NavigateTo(navigationModel.PageKey, navigationModel.Argumment);
                });
                break;
            case ProgressModel progressModel:
                App.TryEnqueue(() =>
                {
                    DialogService.ShowProgress(progressModel);
                });
                break;
            case AppWindowModel appWindowModel:
                switch (appWindowModel.Action)
                {
                    case AppWindowAction.Maximize:
                        ApplicationHelper.Maximize();
                        break;
                    case AppWindowAction.Minimize:
                        ApplicationHelper.Minimize();
                        break;
                    case AppWindowAction.Restore:
                        ApplicationHelper.Restore();
                        break;
                    case AppWindowAction.BringToFront:
                        ApplicationHelper.BringToFront();
                        break;
                }
                break;
        }
    }

}
