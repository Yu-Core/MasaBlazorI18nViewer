namespace MasaBlazorI18nViewer.Rcl.Models
{
    public class TranslationEntry
    {
        public TranslationEntry() { }

        public TranslationEntry(TranslationEntry translationEntry)
        {
            Key = translationEntry.Key;
            Path = [.. translationEntry.Path];
            Translations = new Dictionary<string, string>(translationEntry.Translations);
        }

        public string Key { get; set; } = "";

        public List<string> Path { get; set; }

        // language code -> translation text
        public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
    }
}
