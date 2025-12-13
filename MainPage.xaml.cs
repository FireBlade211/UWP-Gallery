using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage? Current;
        public event EventHandler? NavItemsReady;
        public bool AreNavItemsReady = false;
        private bool CanNavig = true;

        public MainPage()
        {
            InitializeComponent();
            Current = this;

            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
            navManager.BackRequested += NavManager_BackRequested;

            _ = LoadConfig();
        }

        private void NavManager_BackRequested(object? sender, BackRequestedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                e.Handled = true;

                MainFrame.GoBack();
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = MainFrame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Disabled;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationViewItem? settings = NavBar.SettingsItem as NavigationViewItem;

            if (settings != null)
            {
                settings.Icon = new FontIcon
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = new string((char)Symbol.Setting, 1)
                };
            }

            #region Load the navigation view from the JSON file
            foreach (var group in (await ControlInfoDataSource.Instance.GetGroupsAsync()).Where(i => i.IsSpecialSection))
                AddNavBarItemsForGroup(group);

            NavBar.MenuItems.Add(new NavigationViewItemSeparator());

            foreach (var group in ControlInfoDataSource.Instance.Groups.Where(i => !i.IsSpecialSection))
                AddNavBarItemsForGroup(group);

            NavBar.SelectedItem = NavBar.MenuItems.FirstOrDefault(x => x is NavigationViewItem navItem && navItem.Tag.ToString() == "HomeItem");

            AreNavItemsReady = true;
            NavItemsReady?.Invoke(null, new EventArgs());

            #endregion
        }

        private void AddNavBarItemsForGroup(ControlInfoDataGroup group)
        {
            var itemGroup = new NavigationViewItemHeader()
            {
                Content = group.Title,
                Tag = group.UniqueId,
                DataContext = group
            };

            NavBar.MenuItems.Add(itemGroup);

            AutomationProperties.SetName(itemGroup, group.Title);
            AutomationProperties.SetAutomationId(itemGroup, group.UniqueId);

            foreach (var item in group.Items)
            {
                var itemInGroup = new NavigationViewItem()
                {
                    IsEnabled = item.IncludedInBuild,
                    Content = item.Title,
                    Tag = item.UniqueId,
                    DataContext = item,
                    Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = item.IconGlyph
                    }
                };

                var itemInGroupMenuFlyoutItem = new MenuFlyoutItem() { Text = $"Copy Link to {item.Title} sample",
                    Icon = new FontIcon() { Glyph = "\uE8C8", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Tag = item };
                //itemInGroupMenuFlyoutItem.Click += this.OnMenuFlyoutItemClick;
                var shareMenuFlyoutItem = new MenuFlyoutItem() { Command = new StandardUICommand(StandardUICommandKind.Share),
                    Text = $"Share {item.Title} sample", Icon = new FontIcon()
                    { Glyph = "\uE72D", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Tag = item };
                shareMenuFlyoutItem.Click += ShareMenuFlyoutItem_Click;

                itemInGroup.ContextFlyout = new MenuFlyout() { Items = { itemInGroupMenuFlyoutItem, shareMenuFlyoutItem } };

                NavBar.MenuItems.Add(itemInGroup);
                AutomationProperties.SetName(itemInGroup, item.Title);
                AutomationProperties.SetFullDescription(itemInGroup, item.Subtitle);
                AutomationProperties.SetAutomationId(itemInGroup, item.UniqueId);
            }
        }

        private void ShareMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mfi && mfi.Tag is ControlInfoDataItem item)
            {
                App.ShareUIData = item;

                DataTransferManager.ShowShareUI(new ShareUIOptions
                {
                    Theme = ActualTheme == ElementTheme.Light ? ShareUITheme.Light : ShareUITheme.Dark
                });
            }
        }

        private void NavBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (!CanNavig) return;

            if (args.IsSettingsSelected)
            {
                MainFrame.Navigate(typeof(ConfigPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                if (NavBar.SelectedItem is NavigationViewItem nItem)
                {
                    if (nItem.DataContext is ControlInfoDataItem item)
                    {
                        if (item.PageType != null)
                        {
                            MainFrame.Navigate(typeof(ControlPage), item.UniqueId, args.RecommendedNavigationTransitionInfo);
                        }
                    }
                    else if (nItem.Tag.ToString() == "HomeItem")
                    {
                        MainFrame.Navigate(typeof(HomePage), null, args.RecommendedNavigationTransitionInfo);
                    }
                }
            }
        }

        private void NavBar_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            MainFrame.Margin = args.DisplayMode switch
            {
                NavigationViewDisplayMode.Minimal => new(0, 32, 0, 0),
                _ => new(0)
            };
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is ControlInfoDataItem cidi)
            {
                var nvi = (NavigationViewItem?)NavBar.MenuItems.FirstOrDefault(x =>
                {
                    if (x is NavigationViewItem nav)
                    {
                        if (nav.DataContext == cidi)
                        {
                            return true;
                        }
                    }

                    return false;
                });

                nvi?.StartBringIntoView();

                MainFrame.Navigate(typeof(ControlPage), cidi.UniqueId, new DrillInNavigationTransitionInfo());
            }
            else
            {
                MainFrame.Navigate(typeof(SearchResultsPage), args.QueryText, new DrillInNavigationTransitionInfo());
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            CanNavig = false;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = MainFrame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Disabled;

            if (e.Content is ControlPage && e.Parameter is string uid)
            {
                NavBar.SelectedItem = NavBar.MenuItems.FirstOrDefault(x =>
                {
                    if (x is NavigationViewItem nvi)
                    {
                        if (nvi.Tag is string s)
                        {
                            if (s.Equals(uid, StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }, NavBar.SelectedItem);
            }
            else if (e.Content is ConfigPage)
            {
                NavBar.SelectedItem = NavBar.SettingsItem;
            }
            else if (e.Content is HomePage)
            {
                NavBar.SelectedItem = NavBar.MenuItems.FirstOrDefault(x => x is NavigationViewItem navItem && navItem.Tag.ToString() == "HomeItem");
            }
            else
            {
                NavBar.SelectedItem = null;
            }

            CanNavig = true;
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen) return;

            var container = await GallerySearchManager.SearchGallery(SearchBox.Text);
            SearchBox.ItemsSource = container.Items;
        }

        public async Task LoadConfig()
        {
            RequestedTheme = await ConfigurationStorageManager.GetAppTheme();

            var soundProps = await ConfigurationStorageManager.GetSoundProperties();
            ElementSoundPlayer.State = soundProps.IsSoundEnabled ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
            ElementSoundPlayer.Volume = soundProps.Volume;
            ElementSoundPlayer.SpatialAudioMode = soundProps.UseSpatialAudio ? ElementSpatialAudioMode.On : ElementSpatialAudioMode.Off;

            NavBar.PaneDisplayMode = await ConfigurationStorageManager.GetNavigationPaneMode();
        }

        private void MainFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine("Navigation failed!");
        }
    }

    public class NavigationViewDisplayModeStateTrigger : StateTriggerBase
    {
        public NavigationView NavigationView
        {
            get => (NavigationView)GetValue(NavigationViewProperty);
            set
            {
                var oldNavView = (NavigationView)GetValue(NavigationViewProperty);

                oldNavView.DisplayModeChanged -= OnNavViewDisplayModeChanged;

                SetValue(NavigationViewProperty, value);
                value.DisplayModeChanged += OnNavViewDisplayModeChanged;
            }
        }

        private void OnNavViewDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            UpdateTrigger();
        }

        public static readonly DependencyProperty NavigationViewProperty =
            DependencyProperty.Register(nameof(NavigationView), typeof(NavigationView), typeof(NavigationViewDisplayModeStateTrigger),
                new PropertyMetadata(null, OnValueChanged));

        public NavigationViewDisplayMode TargetDisplayMode
        {
            get => (NavigationViewDisplayMode)GetValue(TargetDisplayModeProperty);
            set => SetValue(TargetDisplayModeProperty, value);
        }

        public static readonly DependencyProperty TargetDisplayModeProperty =
            DependencyProperty.Register(nameof(TargetDisplayMode), typeof(NavigationViewDisplayMode), typeof(NavigationViewDisplayModeStateTrigger),
                new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trigger = (NavigationViewDisplayModeStateTrigger)d;
            trigger.UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            SetActive(NavigationView.DisplayMode.Equals(TargetDisplayMode));
        }
    }
}
