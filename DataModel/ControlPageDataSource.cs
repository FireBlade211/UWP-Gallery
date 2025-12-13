using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Shapes;

#pragma warning disable CS8618
#pragma warning disable CS8603
#pragma warning disable CS8602
#pragma warning disable CS8601
#pragma warning disable IL2057

namespace UWPGallery.DataModel
{
    public class Root
    {
        public ObservableCollection<ControlInfoDataGroup> Groups { get; set; }
    }
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(Root))]
    internal partial class RootContext : JsonSerializerContext
    {
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class ControlInfoDataItem
    {
        public string UniqueId { get; set; }
        public string Title { get; set; }
        public string[] BaseClasses { get; set; }
        public string ApiNamespace { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string IconGlyph { get; set; }
        public string BadgeString { get; set; }
        public bool UseGlyph { get; set; }
        public string UrlName { get; set; }
        public string Content { get; set; }
        public bool IsNew { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsPreview { get; set; }
        public ObservableCollection<ControlInfoDocLink> Docs { get; set; }
        public ObservableCollection<string> RelatedControls { get; set; }
        public Type? PageType { get; set; }

        public bool IncludedInBuild { get; set; }

        public string SourcePath { get; set; }

        public override string ToString() => Title;

        // Helpful for XAML data binding
        public ControlInfoDataItem Self => this;

        public Uri GetUrl() => ProtocolActivationClipboardHelper.GetUri(this);

        public ControlInfoRequirements Requirements { get; set; }
    }

    public class ControlInfoRequirements
    {
        public ObservableCollection<ControlInfoApiContract> ApiContracts { get; set; }

        public int? OSBuild { get; set; }

        public List<string> AppCapabilities { get; set; }
    }

    public class ControlInfoDocLink
    {
        public ControlInfoDocLink(string title, string uri)
        {
            Title = title;
            Uri = uri;
        }
        public string Title { get; set; }
        public string Uri { get; set; }
    }

    public class ControlInfoApiContract
    {
        public string Name { get; set; }
        public ControlInfoVersion Version { get; set; }

        public override string ToString() => $"{Name} >= {Version.Major}.{Version.Minor ?? 0}" +
            $"{((Version.Minor != null ? ApiInformation.IsApiContractPresent(Name, (ushort)Version.Major, (ushort)Version.Minor)
                : ApiInformation.IsApiContractPresent(Name, (ushort)Version.Major)) ? " ✓" : string.Empty)}";
    }

    public class ControlInfoVersion
    {
        public int Major { get; set; }
        public int? Minor { get; set; }
    }


    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class ControlInfoDataGroup
    {
        public string UniqueId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string ApiNamespace { get; set; }
        public bool IsSpecialSection { get; set; }
        public string Folder { get; set; }
        public ObservableCollection<ControlInfoDataItem> Items { get; set; }

        public override string ToString() => Title;
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    ///
    /// ControlInfoSource initializes with data read from a static json file included in the
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class ControlInfoDataSource
    {
        private static readonly object _lock = new();

        #region Singleton

        private static readonly ControlInfoDataSource _instance;

        public static ControlInfoDataSource Instance
        {
            get
            {
                return _instance;
            }
        }

        static ControlInfoDataSource()
        {
            _instance = new ControlInfoDataSource();
        }

        private ControlInfoDataSource() { }

        #endregion

        private readonly IList<ControlInfoDataGroup> _groups = new List<ControlInfoDataGroup>();
        public IList<ControlInfoDataGroup> Groups
        {
            get { return _groups; }
        }

        public async Task<IList<ControlInfoDataGroup>> GetGroupsAsync()
        {
            await _instance.GetControlInfoDataAsync();

            return _instance.Groups;
        }

        public static async Task<ControlInfoDataGroup> GetGroupAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _instance.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<ControlInfoDataItem?> GetItemAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _instance.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() > 0) return matches.First();
            return null;
        }

        public static async Task<ControlInfoDataGroup> GetGroupFromItemAsync(string uniqueId)
        {
            await _instance.GetControlInfoDataAsync();
            var matches = _instance.Groups.Where((group) => group.Items.FirstOrDefault(item => item.UniqueId.Equals(uniqueId)) != null);
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetControlInfoDataAsync()
        {
            lock (_lock)
            {
                if (Groups.Count != 0)
                {
                    return;
                }
            }

            var jsonText = await FileLoader.LoadText("DataModel/ControlPageData.json");
            // Strip comments
            var controlInfoDataGroup = JsonSerializer.Deserialize(string.Join("\n", jsonText.ReplaceLineEndings("\n").Split('\n').Where(l => !l.TrimStart().StartsWith("//"))),
                typeof(Root), RootContext.Default) as Root;

            lock (_lock)
            {
                string pageRoot = "UWPGallery.ControlPages.";

                controlInfoDataGroup.Groups.SelectMany(g => g.Items).ToList().ForEach(item =>
                {
#nullable enable
                    string? badgeString = item switch
                    {
                        { IsNew: true } => "New",
                        { IsUpdated: true } => "Updated",
                        { IsPreview: true } => "Preview",
                        _ => null
                    };
                    string pageString = $"{pageRoot}{item.UniqueId}Page";
                    item.PageType = Type.GetType(pageString);

                    item.BadgeString = badgeString;
                    item.IncludedInBuild = item.PageType is not null;
#nullable disable
                });

                foreach (var group in controlInfoDataGroup.Groups)
                {
                    if (!Groups.Any(g => g.Title == group.Title))
                    {
                        Groups.Add(group);
                    }
                }
            }
        }
    }
}
