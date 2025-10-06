using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UWPGallery.DataModel
{
    public class IconData
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string[] Tags { get; set; } = [];

        public string Character => IsSymbolIcon ? new string((char)Enum.Parse<Symbol>(Symbol!), 1) : char.ConvertFromUtf32(Convert.ToInt32(Code, 16));

        public bool IsSymbolIcon { get; set; } = false;
        public string? Symbol { get; set; }

        public string CodeGlyph => "\\u" + Code;
        public string TextGlyph => "&#x" + Code + ";";

        public string DisplayName => (IsSymbolIcon ? Symbol : Name)!;
    }
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(List<IconData>))]
    internal partial class IconDataListContext : JsonSerializerContext
    {
    }

    internal class IconDataSource
    {
        public static IconDataSource Instance { get; } = new();

        public static List<IconData> Icons => Instance.icons;

        private List<IconData> icons = new();

        private IconDataSource() { }

        public object _lock = new();

        public async Task<List<IconData>> LoadIcons()
        {
            lock (_lock)
            {
                if (icons.Count != 0)
                {
                    return icons;
                }
            }
            var jsonText = await FileLoader.LoadText("DataModel/IconData.json");
            lock (_lock)
            {
                if (icons.Count == 0)
                {
                    icons = (JsonSerializer.Deserialize(jsonText, typeof(List<IconData>), IconDataListContext.Default) as List<IconData>)!;
                }
                return icons!;
            }
        }
    }
}
