namespace MasaBlazorI18nViewer.Rcl.Services
{
    public interface IPlatformIntegration
    {
        Task<IFolderObject?> ChooseFolderAsync();
        Task CopyToClipboardAsync(string text);
    }

    public interface IFolderObject : IAsyncDisposable
    {
        Task<IEnumerable<FileObject>> GetFilesAsync(string? fileExtension = null);
        Task SaveFilesAsync(IEnumerable<FileObject> files);
    }

    public class FolderObject : IFolderObject
    {
        public string DirPath { get; set; }

        public FolderObject(string dirPath)
        {
            DirPath = dirPath;
        }

        public async Task<IEnumerable<FileObject>> GetFilesAsync(string? fileExtension = null)
        {
            var searchPattern = fileExtension == null ? "*" : $"*{fileExtension}";
            var files = Directory.GetFiles(DirPath, searchPattern);
            var result = new List<FileObject>();
            foreach (var file in files)
            {
                result.Add(new FileObject
                {
                    FileName = Path.GetFileName(file),
                    Content = await File.ReadAllTextAsync(file)
                });
            }
            return result;
        }

        public async Task SaveFilesAsync(IEnumerable<FileObject> files)
        {
            foreach (var file in files)
            {
                var filePath = Path.Combine(DirPath, file.FileName);
                await File.WriteAllTextAsync(filePath, file.Content);
            }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    public class FileObject
    {
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
