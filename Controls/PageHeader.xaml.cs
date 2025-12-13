using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPGallery.Controls
{
    public sealed partial class PageHeader : UserControl
    {
        public string PageName { get; set; }
        public Action CopyLinkAction { get; set; }

        public ControlInfoDataItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        private ControlInfoDataItem _item;

#pragma warning disable CS8618
        public PageHeader()
        {
            InitializeComponent();
            CopyLinkAction = OnCopyLink;
        }
#pragma warning restore

        public void SetSamplePageSourceLinks(string BaseUri, string PageName)
        {
            // Pagetype is not null!
            // So lets generate the github links and set them!
            var pageName = PageName + ".xaml";
            var code = BaseUri + pageName + ".cs";
            var markup = BaseUri + pageName;

            PageCodeGitHubLink.Tag = code;
            PageCodeGitHubLink.Click += (s, e) => _ = Launcher.LaunchUriAsync(new Uri(code));
            PageMarkupGitHubLink.Tag = markup;
            PageMarkupGitHubLink.Click += (s, e) => _ = Launcher.LaunchUriAsync(new Uri(markup));
        }

        public string GetInheritanceString() => Item?.BaseClasses != null ? string.Join(" > ", Item.BaseClasses) : string.Empty;

        public string GetApiContractString() => (Item != null && Item.Requirements != null && Item.Requirements.ApiContracts != null && Item.Requirements.ApiContracts.Any())
            ? string.Join('\n', Item.Requirements.ApiContracts) : string.Empty;

        public string GetAppCapabililitiesString() => (Item != null && Item.Requirements != null && Item.Requirements.AppCapabilities != null
            && Item.Requirements.AppCapabilities.Any())
            ? string.Join('\n', Item.Requirements.AppCapabilities) : string.Empty;

        private void OnCopyLinkButtonClick(object sender, RoutedEventArgs e)
        {
            CopyLinkAction?.Invoke();
        }

        private void OnCopyLink() => Clipboard.SetContent(ProtocolActivationClipboardHelper.CreateDataPackage(Item));

        public async void OnFeedBackButtonClick(object sender, RoutedEventArgs e) =>
            await Launcher.LaunchUriAsync(new Uri("https://github.com/FireBlade211/UWP-Gallery/issues/new/choose"));

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Item == null || (string.IsNullOrEmpty(Item.ApiNamespace) && Item.BaseClasses == null))
            {
                APIDetailsBtn.Visibility = Visibility.Collapsed;
            }

            if (Item != null)
            {
                FavoriteButton.IsChecked = (await ConfigurationStorageManager.GetFavoriteSamples()).Contains(Item);
            }
        }

        private string GetFavoriteGlyph(bool? isFavorite)
        {
            return (isFavorite ?? false) ? "\uE735" : "\uE734";
        }

        private string GetFavoriteToolTip(bool? isFavorite)
        {
            return (isFavorite ?? false) ? "Remove from favorites" : "Add to favorites";
        }

        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && Item != null)
            {
                if (toggleButton.IsChecked == true)
                {
                    var samples = await ConfigurationStorageManager.GetFavoriteSamples();
                    samples.Add(Item);

                    await ConfigurationStorageManager.SetFavoriteSamples(samples);
                }
                else
                {
                    var samples = await ConfigurationStorageManager.GetFavoriteSamples();
                    samples.Remove(Item);

                    await ConfigurationStorageManager.SetFavoriteSamples(samples);
                }
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            App.ShareUIData = Item;

            DataTransferManager.ShowShareUI();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hlb && hlb.DataContext is string uri)
                _ = Launcher.LaunchUriAsync(new Uri(uri));
        }
    }
}
