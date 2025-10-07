using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<ControlInfoDataItem> AllItems { get; set; } = [];
        public ObservableCollection<ControlInfoDataItem> FavoriteItems { get; set; } = [];
        public ObservableCollection<ControlInfoDataItem> NewSamples { get; set; } = [];
        public ObservableCollection<ControlInfoDataItem> UpdatedSamples { get; set; } = [];

        public HomePage()
        {
            _ = LoadItems();

            InitializeComponent();
        }

        private void AddRange<T>(ICollection<T> target, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                target.Add(item);
            }
        }

        private async Task LoadItems()
        {
            AddRange(AllItems,
                (await ControlInfoDataSource.Instance.GetGroupsAsync()).SelectMany(g => g.Items).Where(i => i.IncludedInBuild).OrderBy(i => i.Title).ToList());

            AddRange(FavoriteItems, await ConfigurationStorageManager.GetFavoriteSamples());

            AddRange(NewSamples, ControlInfoDataSource.Instance.Groups.SelectMany(g => g.Items).Where(i => i.IsNew && i.IncludedInBuild).OrderBy(i => i.Title).ToList());

            AddRange(UpdatedSamples, ControlInfoDataSource.Instance.Groups.SelectMany(g => g.Items).Where(i => i.IsUpdated && i.IncludedInBuild).OrderBy(i => i.Title).ToList());

            favoritesBlock.Visibility = FavoriteItems.Any() ? Visibility.Visible : Visibility.Collapsed;
            favoritesGridView.Visibility = FavoriteItems.Any() ? Visibility.Visible : Visibility.Collapsed;

            newSamplesPanel.Visibility = NewSamples.Any() ? Visibility.Visible : Visibility.Collapsed;
            newSamplesGridView.Visibility = NewSamples.Any() ? Visibility.Visible : Visibility.Collapsed;

            updatedSamplesPanel.Visibility = UpdatedSamples.Any() ? Visibility.Visible : Visibility.Collapsed;
            updatedGridView.Visibility = UpdatedSamples.Any() ? Visibility.Visible : Visibility.Collapsed;

            allSamplesPanel.Visibility = AllItems.Any() ? Visibility.Visible : Visibility.Collapsed;
            allGridView.Visibility = AllItems.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OSVerLabel.Text = $"UWP on {AnalyticsInfo.VersionInfo.ProductName} ({AnalyticsInfo.DeviceForm})";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MainPage.Current != null)
            {
                var groups = await ControlInfoDataSource.Instance.GetGroupsAsync();

                var groupNum = Random.Shared.Next(0, groups.Count);
                var group = groups[groupNum];
                List<ControlInfoDataItem> items = [.. group.Items];

                items.RemoveAll(x => !x.IncludedInBuild);

                var itemNum = Random.Shared.Next(0, items.Count);

                var item = items[itemNum];

                MainPage.Current.NavBar.SelectedItem = MainPage.Current.NavBar.MenuItems.FirstOrDefault(x =>
                {
                    if (x is NavigationViewItem nvi)
                    {
                        if (nvi.Tag is string s)
                        {
                            if (s.Equals(item.UniqueId, StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }, MainPage.Current.NavBar.SelectedItem);
            }
        }

        private void GVItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ControlInfoDataItem item)
            {
                MainPage.Current?.MainFrame.Navigate(typeof(ControlPage), item.UniqueId, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
