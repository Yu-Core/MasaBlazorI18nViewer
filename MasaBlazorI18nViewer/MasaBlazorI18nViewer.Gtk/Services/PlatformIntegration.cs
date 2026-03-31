using MasaBlazorI18nViewer.Rcl.Services;

namespace MasaBlazorI18nViewer.Gtk.Services
{
    public class PlatformIntegration : IPlatformIntegration
    {
        public async Task<IFolderObject?> ChooseFolderAsync()
        {
            var dialog = new global::Gtk.FileChooserNative();
            dialog.Title = "Select Folder";
            dialog.Action = global::Gtk.FileChooserAction.SelectFolder;
            dialog.Modal = true;

            var tcs = new TaskCompletionSource<string?>();

            dialog.OnResponse += (sender, args) =>
            {
                if ((global::Gtk.ResponseType)args.ResponseId == global::Gtk.ResponseType.Accept)
                {
                    tcs.TrySetResult(dialog.GetFile()?.GetPath());
                }
                else
                {
                    tcs.TrySetResult(null);
                }
                dialog.Destroy();
            };

            dialog.Show();
            var folderPath = await tcs.Task;
            if (folderPath == null)
            {
                return null;
            }

            return new FolderObject(folderPath);
        }

        public Task CopyToClipboardAsync(string text)
        {
            var display = global::Gdk.Display.GetDefault();
            var clipboard = display.GetClipboard();
            clipboard.SetText(text);
            return Task.CompletedTask;
        }
    }
}
