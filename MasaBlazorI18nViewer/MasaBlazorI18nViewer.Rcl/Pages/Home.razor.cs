using Masa.Blazor;
using MasaBlazorI18nViewer.Rcl.Models;
using MasaBlazorI18nViewer.Rcl.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MasaBlazorI18nViewer.Rcl.Pages
{
    public partial class Home : IAsyncDisposable
    {
        private string _search = string.Empty;
        private IEnumerable<string> _selected = [];
        private bool showAddOrEditRowDialog = false;
        private bool showImportJsonDialog = false;
        private bool showBaseLanguageDialog = false;
        private string _importJsonContent = string.Empty;
        private IFolderObject? _selectedFolder;
        private bool _isEdit = false;
        private string _editOriginalKey = string.Empty;
        private string _baseLanguage = string.Empty;
        private string BaseLanguage = string.Empty;
        private const string SUPPORTED_CULTURES_JSON = "supportedCultures.json";
        private TranslationEntry _editedItem = new();
        private JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
        // 初始化表头
        private List<DataTableHeader<TranslationEntry>> _headers = [];

        private HashSet<string> languages = [];

        private HashSet<TranslationEntry> i18nData = [];

        private record Item(string Value, string Label, List<Item> Children);

        [Inject]
        private IPopupService PopupService { get; set; } = default!;
        [Inject]
        private IPlatformIntegration PlatformIntegration { get; set; } = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            InitHeaders();
        }

        private void InitHeaders()
        {
            _headers = new()
            {
                new()
                {
                    Text = "键名 (Key)",
                    Value = "Key",
                    Width = "200px" ,
                    CellClass="editable-cell",
                    Fixed = DataTableFixed.Left
                }
            };

            foreach (var item in languages)
            {
                _headers.Add(new DataTableHeader<TranslationEntry>
                {
                    Text = item,
                    Value = item,
                    CellClass = "editable-cell",
                    ValueExpression = it => it.Translations.TryGetValue(item, out var value) ? value : string.Empty
                });
            }

            _headers.Add(new DataTableHeader<TranslationEntry>
            {
                Text = "Actions",
                Value = "actions",
                ValueExpression = it => null,
                Sortable = false,
                Fixed = DataTableFixed.Right
            });
        }
        private void DeleteSelectedRows(MouseEventArgs args)
        {
            foreach (var item in _selected.ToList())
            {
                i18nData.RemoveWhere(it => it.Key == item);
            }

            _selected = [];
        }
        private void AddRow(MouseEventArgs args)
        {
            _isEdit = false;
            _editedItem = new();
            showAddOrEditRowDialog = true;
        }
        private void CloseAddOrEditRow(MouseEventArgs args)
        {
            showAddOrEditRowDialog = false;
            _isEdit = false;
            _editedItem = new();
        }
        private async Task SaveAddOrEditRow()
        {
            if (string.IsNullOrWhiteSpace(_editedItem.Key))
            {
                showAddOrEditRowDialog = false;
                return;
            }

            if (_isEdit)
            {
                if (_editOriginalKey != _editedItem.Key && i18nData.Any(it => it.Key == _editedItem.Key))
                {
                    await PopupService.EnqueueSnackbarAsync("键名已存在，请使用不同的键名。");
                    return;
                }

                var entryToUpdate = i18nData.FirstOrDefault(it => it.Key == _editOriginalKey);
                if (entryToUpdate != null)
                {
                    entryToUpdate.Key = _editedItem.Key;
                    entryToUpdate.Path = _editedItem.Path;
                    entryToUpdate.Translations = _editedItem.Translations;
                }
            }
            else
            {
                if (i18nData.Any(it => it.Key == _editedItem.Key))
                {
                    await PopupService.EnqueueSnackbarAsync("键名已存在，请使用不同的键名。");
                    return;
                }

                foreach (var language in languages)
                {
                    if (!_editedItem.Translations.TryGetValue(language, out string? value) || string.IsNullOrWhiteSpace(value))
                    {
                        _editedItem.Translations[language] = string.Empty;
                    }
                }

                i18nData.Add(new TranslationEntry(_editedItem));
            }

            showAddOrEditRowDialog = false;
            _editedItem = new();
        }
        private async Task ExportMissingItemsToJson(MouseEventArgs args)
        {
            Dictionary<string, Dictionary<string, string>> missingItems = new();
            foreach (var entry in i18nData)
            {
                Dictionary<string, string> missingTranslations = new();
                foreach (var l in languages)
                {
                    if (!entry.Translations.TryGetValue(l, out string? value) || string.IsNullOrWhiteSpace(value))
                    {
                        missingTranslations[l] = string.Empty;
                    }
                }

                if (missingTranslations.Count != 0)
                {
                    missingTranslations[BaseLanguage] = entry.Translations.TryGetValue(BaseLanguage, out var baseValue) ? baseValue : string.Empty;
                    missingItems[entry.Key] = missingTranslations;
                }
            }

            string json = JsonSerializer.Serialize(missingItems, jsonSerializerOptions);
            await PlatformIntegration.CopyToClipboardAsync(json);
            await PopupService.EnqueueSnackbarAsync("缺失的翻译项已复制到剪贴板。");
        }
        private async Task ImportJson(MouseEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_importJsonContent))
            {
                showImportJsonDialog = false;
                return;
            }

            try
            {
                var importItems = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(_importJsonContent, jsonSerializerOptions);
                foreach (var item in importItems)
                {
                    var existingEntry = i18nData.FirstOrDefault(it => it.Key == item.Key);
                    if (existingEntry != null)
                    {
                        foreach (var translation in item.Value)
                        {
                            existingEntry.Translations[translation.Key] = translation.Value;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            showImportJsonDialog = false;
            _importJsonContent = string.Empty;

            await PopupService.EnqueueSnackbarAsync("导入完成。", AlertTypes.Success);
        }
        private void CloseImportJson(MouseEventArgs args)
        {
            showImportJsonDialog = false;
            _importJsonContent = string.Empty;
        }
        private async Task ChooseFolderAsync(MouseEventArgs args)
        {
            PopupService.ShowProgressCircular(options =>
            {
                options.Size = 48;
            });

            var newFolder = await PlatformIntegration.ChooseFolderAsync();
            if (newFolder is null)
            {
                PopupService.HideProgressCircular();
                return;
            }

            await Task.Run(async () =>
            {
                if (_selectedFolder is not null)
                {
                    await _selectedFolder.DisposeAsync();
                }
                _selectedFolder = newFolder;

                var fileNames = new List<string>();
                var locales = new List<(string culture, List<(string Key, string Value, List<string> Path)>)>();

                var jsonFiles = await _selectedFolder.GetFilesAsync(".json");

                var supportedCulturesJson = jsonFiles.FirstOrDefault(f => f.FileName == SUPPORTED_CULTURES_JSON);
                if (supportedCulturesJson is not null)
                {
                    var content = supportedCulturesJson.Content;
                    var cultures = JsonSerializer.Deserialize<string[]>(content);
                    if (cultures is null) return;

                    fileNames.AddRange(cultures.Select(culture => $"{culture}.json"));
                }
                else
                {
                    fileNames.AddRange(jsonFiles.Select(f => f.FileName));
                }

                foreach (var fileName in fileNames)
                {
                    var culture = Path.GetFileNameWithoutExtension(fileName);
                    var fileObject = jsonFiles.FirstOrDefault(f => f.FileName == fileName);
                    if (fileObject is null) continue;
                    var json = fileObject.Content;
                    var locale = new List<(string Key, string Value, List<string> Path)>();
                    using var document = JsonDocument.Parse(json);
                    ParseJsonElement(document.RootElement, new List<string>(), locale);
                    locales.Add((culture, locale));
                }

                HashSet<TranslationEntry> translationEntries = new HashSet<TranslationEntry>();

                foreach (var (culture, locale) in locales)
                {
                    foreach (var item in locale)
                    {
                        var existingEntry = translationEntries.FirstOrDefault(it => it.Key == item.Key);
                        if (existingEntry != null)
                        {
                            existingEntry.Translations[culture] = item.Value;
                        }
                        else
                        {
                            translationEntries.Add(new TranslationEntry
                            {
                                Key = item.Key,
                                Path = item.Path,
                                Translations = new() { { culture, item.Value } }
                            });
                        }
                    }
                }

                languages = locales.Select(l => l.culture).ToHashSet();
                BaseLanguage = languages.Contains("zh-CN")
                    ? "zh-CN"
                    : languages.Contains("en-US")
                        ? "en-US"
                        : languages.FirstOrDefault() ?? string.Empty;
                InitHeaders();
                i18nData = translationEntries;

                PopupService.HideProgressCircular();
            });

            static void ParseJsonElement(JsonElement element, List<string> currentPath, List<(string Key, string Value, List<string> Path)> result)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in element.EnumerateObject())
                    {
                        var newPath = new List<string>(currentPath) { property.Name };
                        ParseJsonElement(property.Value, newPath, result);
                    }
                }
                else if (element.ValueKind == JsonValueKind.String)
                {
                    result.Add((string.Join(".", currentPath), element.GetString() ?? string.Empty, currentPath));
                }
                else
                {
                    result.Add((string.Join(".", currentPath), element.GetRawText(), currentPath));
                }
            }
        }

        // 将当前i18nData中的数据保存到选定的文件夹中，每种语言一个JSON文件，文件名为{culture}.json
        private async Task SaveToFileAsync(MouseEventArgs args)
        {
            if (_selectedFolder is null)
            {
                await PopupService.EnqueueSnackbarAsync("请先选择包含多语言的文件夹", AlertTypes.Warning);
                return;
            }

            try
            {
                PopupService.ShowProgressCircular(options =>
                {
                    options.Size = 48;
                });

                await Task.Run(async () =>
                {
                    var files = new List<FileObject>();

                    foreach (var language in languages)
                    {
                        var root = new Dictionary<string, object>();
                        foreach (var item in i18nData)
                        {
                            if (item.Translations.TryGetValue(language, out var value) && !string.IsNullOrEmpty(value))
                            {
                                var parts = item.Path.ToArray();
                                var current = root;
                                for (int i = 0; i < parts.Length - 1; i++)
                                {
                                    var part = parts[i];
                                    if (!current.TryGetValue(part, out var child) || child is not Dictionary<string, object> childDict)
                                    {
                                        childDict = new Dictionary<string, object>();
                                        current[part] = childDict;
                                    }
                                    current = childDict;
                                }
                                current[parts[^1]] = value;
                            }
                        }

                        var json = JsonSerializer.Serialize(root, jsonSerializerOptions);

                        var fileObject = new FileObject
                        {
                            FileName = $"{language}.json",
                            Content = json
                        };
                        files.Add(fileObject);
                    }

                    if (_selectedFolder is not null)
                    {
                        await _selectedFolder.SaveFilesAsync(files);
                    }

                    await PopupService.EnqueueSnackbarAsync("保存成功", AlertTypes.Success);
                });
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync($"保存失败: {ex.Message}", AlertTypes.Error);
            }
            finally
            {
                PopupService.HideProgressCircular();
            }
        }

        private void ClearSelectedRows(MouseEventArgs args)
        {
            foreach (var item in _selected.ToList())
            {
                var translationEntry = i18nData.FirstOrDefault(it => it.Key == item);
                translationEntry?.Translations = [];
            }

            _selected = [];
        }

        private List<Item> _pathItems = [];

        private void HandleEditedItemKeyOnChange(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _pathItems = [];
                return;
            }

            _pathItems = GetHierarchicalList(value);
            _editedItem.Path = value.Split('.').ToList();

            static List<Item> GetHierarchicalList(string value)
            {
                if (string.IsNullOrEmpty(value)) return [];

                var parts = value.Split('.');
                return GenerateItems(parts, 0);
            }

            static List<Item> GenerateItems(string[] parts, int startIndex)
            {
                var list = new List<Item>();
                if (startIndex >= parts.Length) return list;

                string currentLabel = string.Empty;
                for (int i = startIndex; i < parts.Length; i++)
                {
                    if (i == startIndex)
                    {
                        currentLabel = parts[i];
                    }
                    else
                    {
                        currentLabel += "." + parts[i];
                    }

                    var children = GenerateItems(parts, i + 1);
                    list.Add(new Item(currentLabel, currentLabel, children));
                }
                return list;
            }
        }

        private void EditItem(TranslationEntry translationEntry)
        {
            _isEdit = true;
            _editOriginalKey = translationEntry.Key;
            _editedItem = new TranslationEntry(translationEntry);
            HandleEditedItemKeyOnChange(_editedItem.Key);
            showAddOrEditRowDialog = true;
        }

        private void DeleteItem(TranslationEntry translationEntry)
        {
            i18nData.RemoveWhere(it => it.Key == translationEntry.Key);
        }

        private void SaveBaseLanguage()
        {
            BaseLanguage = _baseLanguage;
            showBaseLanguageDialog = false;
        }

        private void CloseBaseLanguage()
        {
            showBaseLanguageDialog = false;
            _baseLanguage = BaseLanguage;
        }
        private void OpenBaseLanguageDialog(MouseEventArgs args)
        {
            _baseLanguage = BaseLanguage;
            showBaseLanguageDialog = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_selectedFolder is not null)
            {
                await _selectedFolder.DisposeAsync();
                _selectedFolder = null;
            }
        }
    }
}