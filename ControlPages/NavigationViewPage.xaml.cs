using System;
using System.Collections.ObjectModel;
using System.Linq;
using UWPGallery.SamplePages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationViewPage : Page
    {
        public ObservableCollection<MyNavViewDataBindItem> NavViewDataBindCollection { get; set; } = [];
        public bool BackNavigNavViewNavigLock = false;

        public NavigationViewPage()
        {
            InitializeComponent();

            for (int i = 1; i < 6; i++)
            {
                var item = new MyNavViewDataBindItem();
                item.Text = $"Data-Bound Item {i}";
                item.Index = i;

                NavViewDataBindCollection.Add(item);
            }
        }

        private void LeftNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            LeftNavView.Header = (LeftNavView.SelectedItem as NavigationViewItem)!.Content.ToString() + " Page";
        }

        private void TopNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
                TopNavView.Header = "Settings Page";
            else
                TopNavView.Header = (TopNavView.SelectedItem as NavigationViewItem)!.Content.ToString() + " Mail";
        }

#pragma warning disable IL2057
        private void TabsNavViewSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var options = new FrameNavigationOptions();
            options.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            options.IsNavigationStackEnabled = false; // DON'T ADD THE PAGE TO THE NAVIGATION STACK

            Type? pageType = null;

            if (args.IsSettingsSelected)
                pageType = typeof(SampleSettingsPage);
            else
            {
                var navItem = TabsNavViewSample.SelectedItem as NavigationViewItem;

                if (navItem != null)
                {
                    if (navItem.Tag is string s)
                    {
                        pageType = Type.GetType("UWPGallery.SamplePages." + s);
                    }
                }
            }

            if (pageType != null)
            {
                TabsNavViewSampleFrame.NavigateToType(pageType, null, options);
            }
        }

        private void DataBindNavViewSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame4.Navigate(typeof(SampleSettingsPage));
            }
            else
            {
                var selectedItem = (MyNavViewDataBindItem)args.SelectedItem;
                string pageName = "UWPGallery.SamplePages." + $"SamplePage{selectedItem.Index}";
                Type? pageType = Type.GetType(pageName);
                contentFrame4.Navigate(pageType);
            }
        }

        private void ItemHeadersSampleNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (ItemHeadersSampleNavView.SelectedItem is NavigationViewItem item)
            {
                if (item.Tag is string s)
                {
                    string pageName = "UWPGallery.SamplePages." + s;
                    Type? pageType = Type.GetType(pageName);

                    if (pageType != null)
                    {
                        ItemHeadersSampleNavViewFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
                    }
                }
            }
        }

        private void ItemHeadersSampleNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer.Tag.ToString() == "NavViewRotate")
            {
                if (ItemHeadersSampleNavView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                {
                    ItemHeadersSampleNavViewRotateItem.Content = "Move to Top";
                    ItemHeadersSampleNavViewRotateItem.Icon = new SymbolIcon(Symbol.Up);

                    ItemHeadersSampleNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                }
                else
                {
                    ItemHeadersSampleNavViewRotateItem.Content = "Move to Left";
                    ItemHeadersSampleNavViewRotateItem.Icon = new SymbolIcon(Symbol.Back);

                    ItemHeadersSampleNavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                }
            }
        }

        private void BackNavigSampleNavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            BackNavigSampleNavViewFrame.GoBack();
        }

        private void BackNavigSampleNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // prevent the rest of the code from running if the lock is set
            if (BackNavigNavViewNavigLock) return;

            if (args.IsSettingsSelected)
                BackNavigSampleNavViewFrame.Navigate(typeof(SampleSettingsPage), null, args.RecommendedNavigationTransitionInfo);
            else
            {
                if (BackNavigSampleNavView.SelectedItem is NavigationViewItem item)
                {
                    if (item.Tag is string s)
                    {
                        Type? pageType = Type.GetType("UWPGallery.SamplePages.SamplePage" + s);

                        if (pageType != null)
                        {
                            BackNavigSampleNavViewFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
                        }
                    }
                }
            }
        }

        private void BackNavigSampleNavViewFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // lock to prevent the navview from triggering SelectionChanged and navigating again
            BackNavigNavViewNavigLock = true;

            BackNavigSampleNavView.SelectedItem = e.Content switch // Use Content so that we can switch directly because you can only use constants in switch so using SourcePageType with typeof wouldn't work
            {
                SamplePage1 => BackNavigSampleHomeItem,
                SamplePage2 => BackNavigSampleAccountItem,
                SamplePage5 => BackNavigSampleToolsItem,
                SampleSettingsPage => BackNavigSampleNavView.SettingsItem, // remove this if using a navigationview with IsSettingsVisible = false
                _ => null
            };

            // revert the lock back
            BackNavigNavViewNavigLock = false;
        }

        private void NavViewSearchBoxSample_Loaded(object sender, RoutedEventArgs e)
        {
            var items = NavViewSearchBoxSample.MenuItems.Select(string (object navItem) =>
            {
                if (navItem is NavigationViewItem item)
                {
                    return item.Content.ToString() ?? string.Empty;
                }

                return string.Empty;
            }).ToList();

            items.Add("Settings");

            SearchBox.ItemsSource = items;
        }

        private void NavViewSearchBoxSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
                NavViewSearchBoxSampleFrame.Navigate(typeof(SampleSettingsPage), null, args.RecommendedNavigationTransitionInfo);
            else
            {
                if (NavViewSearchBoxSample.SelectedItem is NavigationViewItem item)
                {
                    if (item.Tag is string s)
                    {
                        Type? pageType = Type.GetType("UWPGallery.SamplePages.SamplePage" + s);

                        if (pageType != null)
                        {
                            NavViewSearchBoxSampleFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
                        }
                    }
                }
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var querySplit = SearchBox.Text.ToLower().Split(" ");

            var items = NavViewSearchBoxSample.MenuItems.Select(string (object navItem) =>
            {
                if (navItem is NavigationViewItem item)
                {
                    return item.Content.ToString() ?? string.Empty;
                }

                return string.Empty;
            }).ToList();

            items.Add("Settings");

            SearchBox.ItemsSource = items.Where(x =>
            {
                // Idea: check for every word entered (separated by space) if it is in the name,
                // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                bool flag = true;
                foreach (string queryToken in querySplit)
                {
                    // Check if token is in name
                    if (!x.Contains(queryToken, StringComparison.OrdinalIgnoreCase))
                    {
                        // The title doesn't contain one of the tokens so we discard this item!
                        flag = false;
                    }
                }
                return flag;
            }).ToList();
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is string s)
            {
                if (s.Equals("Settings", StringComparison.Ordinal))
                {
                    NavViewSearchBoxSample.SelectedItem = NavViewSearchBoxSample.SettingsItem;
                }
                else
                {
                    foreach (var item in NavViewSearchBoxSample.MenuItems)
                    {
                        if (item is NavigationViewItem navItem)
                        {
                            if (navItem.Content is string ss)
                            {
                                if (s.Equals(ss, StringComparison.Ordinal))
                                {
                                    NavViewSearchBoxSample.SelectedItem = navItem;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
#pragma warning restore
    }

    public class MyNavViewDataBindItem
    {
        public string Text;
        public int Index;

        public SymbolIcon GetRandomIcon()
        {
            var icons = Enum.GetValues<Symbol>();

            var index = Random.Shared.Next(0, icons.Length);

            return new SymbolIcon(icons.ElementAt(index));
        }

    }
}
