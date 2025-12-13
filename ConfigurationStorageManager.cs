using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPGallery
{
    [JsonSerializable(typeof(ConfigurationDataObject))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    internal partial class ConfigurationContext : JsonSerializerContext { }

    /// <summary>
    /// Manages the storage and retreival of configuation values.
    /// </summary>
    // Performance and disk usage could probably be improved by caching the JSON and writing it at app shutdown
    public static class ConfigurationStorageManager
    {
        /// <summary>
        /// Gets the path to the currently used configuration file.
        /// </summary>
        public static string ConfigFilePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, "config.json");

        private async static Task<ConfigurationDataObject> DeserializeDataObjectAsync()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists).AsTask();

            using (Stream stream = await (await StorageFile.GetFileFromPathAsync(ConfigFilePath).AsTask()).OpenStreamForReadAsync())
            {
                try
                {
                    ConfigurationDataObject? dataObj = JsonSerializer.Deserialize(stream, ConfigurationContext.Default.ConfigurationDataObject);

                    if (dataObj != null) return dataObj;
                }
                catch { }
            }

            return new ConfigurationDataObject();
        }

        private async static Task SerializeAndSaveDataObjectAsync(ConfigurationDataObject dataObj)
        {
            _ = await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.ReplaceExisting).AsTask();

            try
            {
                await FileIO.WriteTextAsync(await StorageFile.GetFileFromPathAsync(ConfigFilePath),
                JsonSerializer.Serialize(dataObj, ConfigurationContext.Default.ConfigurationDataObject));
            }
            catch { }
        }

        /// <summary>
        /// Gets the collection of favorited samples by the user.
        /// </summary>
        /// <returns>The collection of favorited samples by the user.</returns>
        public async static Task<List<ControlInfoDataItem>> GetFavoriteSamples()
        {
            await ControlInfoDataSource.Instance.GetGroupsAsync();

            var samples = new List<ControlInfoDataItem>();

            var dobj = await DeserializeDataObjectAsync();

            var uids = dobj.FavoriteSampleUIDs;

            foreach (string uid in uids)
            {
                ControlInfoDataItem? item = await ControlInfoDataSource.GetItemAsync(uid);

                if (item != null)
                {
                    samples.Add(item);
                }
            }

            return samples;
        }

        /// <summary>
        /// Sets the collection of favorited samples by the user.
        /// </summary>
        /// <param name="items">The collection to set the value to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        public async static Task SetFavoriteSamples(List<ControlInfoDataItem> items)
        {
            var uids = items.Select(item => item.UniqueId);

            var dobj = await DeserializeDataObjectAsync();
            dobj.FavoriteSampleUIDs = [.. uids];

            await SerializeAndSaveDataObjectAsync(dobj);
        }

        ///// <summary>
        ///// Gets the start page of the gallery.
        ///// </summary>
        ///// <remarks>If this function returns <see langword="null"/> you should treat it as a command to use the home page.</remarks>
        ///// <returns>The currently set start page of the gallery, or <see langword="null"/>. The gallery should start up on this page, or on the home page if <see langword="null"/> is returned.</returns>
        //public async static Task<ControlInfoDataItem?> GetGalleryStartPage()
        //{
        //    await ControlInfoDataSource.Instance.GetGroupsAsync();

        //    var dobj = await DeserializeDataObjectAsync();

        //    ControlInfoDataItem? item = await ControlInfoDataSource.GetItemAsync(dobj.GalleryStartPageUID);

        //    return item;
        //}

        ///// <summary>
        ///// Sets the start page of the gallery to the home page.
        ///// </summary>
        ///// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        //public async static Task SetGalleryStartPage()
        //{
        //    var dobj = await DeserializeDataObjectAsync();

        //    dobj.GalleryStartPageUID = string.Empty;

        //    await SerializeAndSaveDataObjectAsync(dobj);
        //}

        ///// <summary>
        ///// Sets the start page of the gallery to the specified page.
        ///// </summary>
        ///// <param name="item">The item whose page should be the default when starting up.</param>
        ///// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        //public async static Task SetGalleryStartPage(ControlInfoDataItem item)
        //{
        //    var dobj = await DeserializeDataObjectAsync();

        //    dobj.GalleryStartPageUID = item.UniqueId;

        //    await SerializeAndSaveDataObjectAsync(dobj);
        //}

        /// <summary>
        /// Gets the currently used app theme from the configuration file.
        /// </summary>
        /// <remarks>This is not the currently used theme, but the theme that SHOULD be currently used as indicated by the user's preferences.</remarks>
        /// <returns>The currently used app theme.</returns>
        public async static Task<ElementTheme> GetAppTheme()
        {
            var dobj = await DeserializeDataObjectAsync();

            return dobj.AppTheme;
        }

        /// <summary>
        /// Sets the user's preference for the application theme.
        /// </summary>
        /// <param name="theme">The new value of the setting.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        public async static Task SetAppTheme(ElementTheme theme)
        {
            var dobj = await DeserializeDataObjectAsync();
            dobj.AppTheme = theme;

            await SerializeAndSaveDataObjectAsync(dobj);
        }

        /// <summary>
        /// Gets the current sound properties that should be used.
        /// </summary>
        /// <returns>The sound properties that should be used.</returns>
        public async static Task<ConfigurationSoundProperties> GetSoundProperties()
        {
            var dobj = await DeserializeDataObjectAsync();

            return dobj.SoundProperties;
        }

        /// <summary>
        /// Sets the current sound properties in the configuration file.
        /// </summary>
        /// <param name="props">The new properties.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        public async static Task SetSoundProperties(ConfigurationSoundProperties props)
        {
            var dobj = await DeserializeDataObjectAsync();
            dobj.SoundProperties = props;

            await SerializeAndSaveDataObjectAsync(dobj);
        }
        /// <summary>
        /// Gets the most recently loaded Scratch Pad XAML content.
        /// </summary>
        /// <returns>The most recently loaded Scratch Pad XAML content.</returns>
        public async static Task<string> GetLastScratchPadXaml()
        {
            var dobj = await DeserializeDataObjectAsync();
            return dobj.LastScratchPadXaml;
        }

        /// <summary>
        /// Sets the most recently loaded Scratch Pad XAML content in the configuration file.
        /// </summary>
        /// <param name="xaml">The XAML content.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        public async static Task SetLastScratchPadXaml(string xaml)
        {
            var dobj = await DeserializeDataObjectAsync();
            dobj.LastScratchPadXaml = xaml;
            await SerializeAndSaveDataObjectAsync(dobj);
        }
        /// <summary>
        /// Gets the currently set navigation pane mode in the configuration file.
        /// </summary>
        /// <returns>The currently set navigation pane mode in the configuration file.</returns>
        public async static Task<NavigationViewPaneDisplayMode> GetNavigationPaneMode()
        {
            var dobj = await DeserializeDataObjectAsync();
            return dobj.NavigationPaneMode;
        }

        /// <summary>
        /// Sets the navigation pane mode in the configuration file.
        /// </summary>
        /// <param name="mode">The new mode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        public async static Task SetNavigationPaneMode(NavigationViewPaneDisplayMode mode)
        {
            var dobj = await DeserializeDataObjectAsync();
            dobj.NavigationPaneMode = mode;
            await SerializeAndSaveDataObjectAsync(dobj);
        }
    }

    /// <summary>
    /// Represents the JSON data object used to store the configuration data from <see cref="ConfigurationStorageManager"/>.
    /// </summary>
    public class ConfigurationDataObject
    {
        /// <summary>
        /// Specifies the UIDs of the samples that are set as favorite by the user.
        /// </summary>
        public List<string> FavoriteSampleUIDs { get; set; } = [];

        ///// <summary>
        ///// Specifies the UID of the page to start the gallery on. If set to <see cref="string.Empty"/>, defaults to the Home page.
        ///// </summary>
        //public string GalleryStartPageUID { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the theme of the app to use.
        /// </summary>
        public ElementTheme AppTheme { get; set; } = ElementTheme.Default;

        /// <summary>
        /// Specifies the sound properties.
        /// </summary>
        public ConfigurationSoundProperties SoundProperties { get; set; } = new();
        /// <summary>
        /// Specifies the most recently loaded Scratch Pad XAML content.
        /// </summary>
        public string LastScratchPadXaml { get; set; } = string.Empty;
        /// <summary>
        /// Specifies the current NavigationView pane mode.
        /// </summary>
        public NavigationViewPaneDisplayMode NavigationPaneMode { get; set; } = NavigationViewPaneDisplayMode.Auto;
    }

    /// <summary>
    /// Specifies sound properties for <see cref="ConfigurationDataObject"/>.
    /// </summary>
    public class ConfigurationSoundProperties
    {
        /// <summary>
        /// Specifies whether sound is enabled.
        /// </summary>
        public bool IsSoundEnabled { get; set; } = false;

        /// <summary>
        /// Specifies the volume of the sound effects.
        /// </summary>
        public double Volume { get; set; } = 1.0;

        /// <summary>
        /// Specifies whether to use spatial audio.
        /// </summary>
        public bool UseSpatialAudio { get; set; } = false;
    }
}
