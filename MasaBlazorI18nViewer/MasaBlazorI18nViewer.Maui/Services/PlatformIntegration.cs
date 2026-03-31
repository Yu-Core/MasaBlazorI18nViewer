using CommunityToolkit.Maui.Storage;
using MasaBlazorI18nViewer.Rcl.Services;

namespace MasaBlazorI18nViewer.Maui.Services
{
    public class PlatformIntegration : IPlatformIntegration
    {
        public async Task<IFolderObject?> ChooseFolderAsync()
        {
            try
            {
                var result = await FolderPicker.Default.PickAsync();
                var folderPath = result.Folder?.Path;
                if (folderPath == null)
                {
                    return null;
                }

                return new FolderObject(folderPath);
            }
            catch (Exception ex)
            {
                // handle permission issues or cancellation
                return null;
            }
        }

        public Task CopyToClipboardAsync(string text)
        {
            return Clipboard.Default.SetTextAsync(text);
        }
    }
}