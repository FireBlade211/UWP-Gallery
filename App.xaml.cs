using FireBlade.CsTools;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.Controls;
using UWPGallery.DataModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;

namespace UWPGallery
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
    /// </summary>
    public sealed partial class App : Application
    {
        public static ControlInfoDataItem? ShareUIData;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Suspending += OnSuspending;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);

            SettingsPane.GetForCurrentView().CommandsRequested += SettingsPane_CommandsRequested;
            DataTransferManager.GetForCurrentView().DataRequested += DataTransferManager_DataRequested;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (ShareUIData != null)
            {
                var pkg = ProtocolActivationClipboardHelper.CreateDataPackage(ShareUIData);
                pkg.Properties.Description = "Share this sample";

                args.Request.Data = pkg;
            }
            else
            {
                args.Request.FailWithDisplayText("The sharing operation could not proceed because no data packages were assigned or could be found.");
            }
        }

        private void SettingsPane_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var generalCmd = new SettingsCommand(0, "General", (IUICommand cmd) =>
            {
                _ = new MessageDialog("You clicked the General command!", "Settings Pane").ShowAsync();
            });

            var aboutCmd = new SettingsCommand(0, "About", (IUICommand cmd) =>
            {
                _ = new MessageDialog("You clicked the About command!", "Settings Pane").ShowAsync();
            });

            args.Request.ApplicationCommands.Add(generalCmd);
            args.Request.ApplicationCommands.Add(aboutCmd);
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            InitApp();

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as ProtocolActivatedEventArgs;

                if (protocolArgs != null)
                {
                    Uri uri = protocolArgs.Uri;

                    // Example: "uwp-gallery:///page/12345#section1"
                    string trimAbsPath = uri.AbsolutePath.TrimStart('/');
                    string path = trimAbsPath.Split('/').Last(); // "12345"
                    string host = trimAbsPath.Split('/').First();
                    string frag = uri.Fragment.TrimStart('#');     // "section1"

                    //Debugger.Launch();

                    Debug.WriteLine("URI data:");
                    foreach (var prop in typeof(Uri).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        Debug.WriteLine($"{prop.Name} = {prop.GetValue(uri)}");
                    }

                    Debug.WriteLine($"Trimmed abs path: {trimAbsPath}");
                    Debug.WriteLine($"Path: {path}");
                    Debug.WriteLine($"Host: {host}");
                    Debug.WriteLine($"Fragment: {frag}");

                    if (host.Equals("page", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("Main page " + (MainPage.Current != null ? "exists!" : "is null!"));

                        if (MainPage.Current != null)
                        {
                            ControlInfoDataItem? item = null;

                            // get groups async gives the existing collection if they have already been fetched
                            foreach (var group in await ControlInfoDataSource.Instance.GetGroupsAsync())
                            {
                                Debug.WriteLine($"Iterating groups: {group.Title}");
                                var i = group.Items.FirstOrDefault(x => x.UrlName.Equals(path, StringComparison.OrdinalIgnoreCase));

                                if (i != null)
                                {
                                    Debug.WriteLine($"Item found! {i.Title} (uri: {i.UrlName})");
                                    item = i;
                                    break;
                                }
                            }

                            if (item != null)
                            {
                                void DoLoadItem()
                                {
                                    if (MainPage.Current != null)
                                    {
                                        if (frag.IsNotNullOrEmpty())
                                        {
                                            void OnNavigated(object s, NavigationEventArgs e)
                                            {
                                                Debug.WriteLine("On navigated!");

                                                if (MainPage.Current != null)
                                                    MainPage.Current.MainFrame.Navigated -= OnNavigated;

                                                Debug.WriteLine("Source page type is proper equals!");
                                                if (e.Content is ControlPage page)
                                                {
                                                    Debug.WriteLine("Content is control page!");

                                                    page.contentFrame.Navigated += (s, e) =>
                                                    {
                                                        var sample = GetGallerySampleInPage((Page)e.Content, frag);

                                                        Debug.WriteLineIf(sample == null, "Sample is null!");

                                                        if (sample != null)
                                                        {
                                                            Debug.WriteLine("Sample is not null!");
                                                            Debug.WriteLine($"Sample loaded: {sample.IsLoaded}");

                                                            if (sample.IsLoaded)
                                                            {
                                                                sample.StartBringIntoView();
                                                                sample.Focus(FocusState.Programmatic);
                                                            }
                                                            else sample.Loaded += (s, e) =>
                                                            {
                                                                sample.StartBringIntoView();
                                                                sample.Focus(FocusState.Programmatic);
                                                            };
                                                        }
                                                    };
                                                }
                                            }

                                            MainPage.Current.MainFrame.Navigated += OnNavigated;

                                            var navItem = MainPage.Current.NavBar.MenuItems.FirstOrDefault(
                                                x => x is NavigationViewItem navItem && navItem.Tag is string uid && uid == item.UniqueId);

                                            Debug.WriteLineIf(navItem != null, "Navigation item found!");
                                            Debug.WriteLineIf(navItem == null, "Navigation item is null...");

                                            MainPage.Current.NavBar.SelectedItem = navItem;

    
                                        }
                                        else
                                        {
                                            MainPage.Current.NavBar.SelectedItem = MainPage.Current.NavBar.MenuItems.FirstOrDefault(
                                                x => x is NavigationViewItem navItem && navItem.Tag is string uid && uid == item.UniqueId);
                                        }
                                    }
                                }

                                if (MainPage.Current.AreNavItemsReady) DoLoadItem();
                                else MainPage.Current.NavItemsReady += (s, e) => DoLoadItem();
                            }
                        }
                    }
                }
            }

            base.OnActivated(args);
        }

        private GallerySample? GetGallerySampleInPage(DependencyObject page, string headerTitle)
        {
            int count = VisualTreeHelper.GetChildrenCount(page);
            Debug.WriteLine("Iterating visual tree! Children: " + count);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(page, i);

                Debug.WriteLine($"Child found: {(child is FrameworkElement element ? element.Name : "Unnamed")}");

                // Check type
                if (child is GallerySample sample)
                {
                    Debug.WriteLine($"Gallery sample found! Title: {sample.HeaderTitle}");

                    if (sample.SampleUrl.Equals(headerTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("Gallery sample matches!");

                        return sample;
                    }
                }

                // Recurse
                var result = GetGallerySampleInPage(child, headerTitle);
                if (result != null) return result;
            }

            return null;
        }

        private void InitApp(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page, configuring
                    // the new page by passing required information as a navigation parameter.
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void InitApp()
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            InitApp(e);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails.
        /// </summary>
        /// <param name="sender">The Frame which failed navigation.</param>
        /// <param name="e">Details about the navigation failure.</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page '{e.SourcePageType.FullName}'.");
        }

        /// <summary>
        /// Invoked when application execution is being suspended. Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    public partial class EmptyStringToVisibilityConverter : IValueConverter
    {
        public Visibility EmptyValue { get; set; } = Visibility.Collapsed;
        public Visibility NonEmptyValue { get; set; } = Visibility.Visible;


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return EmptyValue;
            }
            else if (value is string stringValue && stringValue != "")
            {
                return NonEmptyValue;
            }
            else
            {
                return EmptyValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
