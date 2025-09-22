using System.Collections.Generic;
using UWPGallery.DataModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchResultsPage : Page
    {
        public SearchResultsPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string query)
            {
                MainPivot.Title = "SEARCH RESULTS FOR " + query.ToUpper();

                var results = await GallerySearchManager.SearchGallery(query);

                int count = 0;

                foreach (var category in results.Categories)
                {
                    if (category.ShouldShow)
                    {
                        var pi = new PivotItem();
                        pi.Header = category.DisplayHeader;
                        var gv = new GridView
                        {
                            Margin = new Thickness(12, 24, 0, 0),
                            Padding = new Thickness(0, 0, 0, 36),
                            IsItemClickEnabled = true,
                            IsSwipeEnabled = false,
                            SelectionMode = ListViewSelectionMode.None,
                            ItemTemplate = Application.Current.Resources["ControlItemTemplate"] as DataTemplate,
                            ItemsSource = category.Items
                        };

                        AutomationProperties.SetName(gv, "Search Results");

                        gv.ItemClick += AllGV_ItemClick;

                        pi.Content = gv;

                        MainPivot.Items.Add(pi);

                        if (AllGV.ItemsSource is List<ControlInfoDataItem> l)
                            l.AddRange(category.Items);
                        else
                        {
                            var list = new List<ControlInfoDataItem>();
                            list.AddRange(category.Items);

                            AllGV.ItemsSource = list;
                        }

                        count += category.Count;
                    }
                }

                AllItem.Header = "All (" + count + ")";
                
                if (count == 0)
                {
                    AllGV.Visibility = Visibility.Collapsed;
                    NoResultsTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void AllGV_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ControlInfoDataItem item)
            {
                MainPage.Current?.MainFrame.Navigate(typeof(ControlPage), item.UniqueId, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
