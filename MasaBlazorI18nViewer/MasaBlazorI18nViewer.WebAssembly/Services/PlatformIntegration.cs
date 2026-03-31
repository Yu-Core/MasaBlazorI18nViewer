using MasaBlazorI18nViewer.Rcl.Services;
using Microsoft.JSInterop;

namespace MasaBlazorI18nViewer.WebAssembly.Services
{
    public class PlatformIntegration : IPlatformIntegration
    {
        private readonly IJSRuntime _jsRuntime;

        public PlatformIntegration(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<IFolderObject?> ChooseFolderAsync()
        {
            try
            {
                var dictHandle = await _jsRuntime.InvokeAsync<IJSObjectReference>("chooseFileSystemFolder");
                if (dictHandle == null) return null;
                return new WebFolderObject(dictHandle);
            }
            catch
            {
                return null;
            }
        }

        public async Task CopyToClipboardAsync(string text)
        {
            await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }

    public class WebFolderObject : IFolderObject
    {
        private readonly IJSObjectReference _jsObjectReference;

        public WebFolderObject(IJSObjectReference jsObjectReference)
        {
            _jsObjectReference = jsObjectReference;
        }

        public async Task<IEnumerable<FileObject>> GetFilesAsync(string? fileExtension = null)
        {
            return await _jsObjectReference.InvokeAsync<IEnumerable<FileObject>>("getFiles", fileExtension);
        }

        public async Task SaveFilesAsync(IEnumerable<FileObject> files)
        {
            await _jsObjectReference.InvokeVoidAsync("saveFiles", files);
        }

        public async ValueTask DisposeAsync()
        {
            if (_jsObjectReference != null)
            {
                await _jsObjectReference.DisposeAsync();
            }
        }
    }
}
