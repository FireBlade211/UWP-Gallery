using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ControlPage : Page
    {
        private static string GalleryBaseUrl = "https://github.com/FireBlade211/UWP-Gallery/tree/main/ControlPages/";

        public ControlInfoDataItem Item
        {
            get { return _item; }
            set { _item = value; }
        }

        private ControlInfoDataItem _item;

#pragma warning disable CS8618
        public ControlPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Focus(FocusState.Programmatic);

                _ = LoadRelatedControls();
            };
        }
#pragma warning restore

        private async Task LoadRelatedControls()
        {
            if (Item == null) return;
            if (Item.RelatedControls == null) return;

            var col = new ObservableCollection<ControlInfoDataItem>();

            await ControlInfoDataSource.Instance.GetGroupsAsync();

            foreach (var uid in Item.RelatedControls)
            {
                var item = await ControlInfoDataSource.GetItemAsync(uid);

                if (item != null)
                {
                    col.Add(item);
                }
            }

            RelatedItemsControl.ItemsSource = col;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var uniqueId = (string)e.Parameter;
            var group = await ControlInfoDataSource.GetGroupFromItemAsync(uniqueId);
            var item = group?.Items.FirstOrDefault(x => x.UniqueId.Equals(uniqueId));

            if (item != null)
            {
                Item = item;

                if (group != null)
                {
                    var pageName = string.IsNullOrEmpty(group.Folder) ? item.PageType?.Name : $"{group.Folder}/{item.PageType?.Name}";
                    pageHeader.SetSamplePageSourceLinks(GalleryBaseUrl, pageName!);
                    Debug.WriteLine(string.Format("[ControlPage] Navigate to {0}", item.PageType?.ToString()));
                    contentFrame.Navigate(item.PageType);
                }
            }

            base.OnNavigatedTo(e);
        }

#pragma warning disable IL2075
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // We use reflection to call the OnNavigatedFrom function the user leaves this page
            // See this PR for more information: https://github.com/microsoft/WinUI-Gallery/pull/145
            Page? innerPage = contentFrame.Content as Page;
            if (innerPage != null)
            {
                MethodInfo? dynMethod = innerPage.GetType().GetMethod("OnNavigatedFrom",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                dynMethod?.Invoke(innerPage, [e]);
            }

            base.OnNavigatedFrom(e);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hlb && hlb.DataContext is Uri uri)
            {
                _ = Launcher.LaunchUriAsync(uri);
            }
        }
#pragma warning restore
    }
}
