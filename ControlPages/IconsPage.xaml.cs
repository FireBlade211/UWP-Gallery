using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UWPGallery.DataModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IconsPage : Page
    {
        public IconsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(async () =>
            {
                await IconDataSource.Instance.LoadIcons();

                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainGV.ItemsSource = IconDataSource.Icons;
                    mainGV.SelectedItem = IconDataSource.Icons.First();

                    iconCountBlock.Text = $"{IconDataSource.Icons.Count} icons";
                });
            }).Start();

            //var str = "{\n";

            //foreach (var symbol in Enum.GetNames<Symbol>())
            //    str += $"   \"{symbol}\",\n";

            //var dlg = new ContentDialog()
            //{
            //    Content = new TextBlock
            //    {
            //        IsTextSelectionEnabled = true,
            //        Text = str
            //    },
            //    Title = "Symbols",
            //    PrimaryButtonText = "OK",
            //    DefaultButton = ContentDialogButton.Primary
            //};

            //_ = dlg.ShowAsync();
        }

        private void mainGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainGV.SelectedItem is IconData icod)
            {
                IconNameLargeBlock.Text = icod.DisplayName;
                IconNameBlock.Text = icod.DisplayName;
                PreviewIcon.Glyph = icod.Character;
                TagsItemsControl.ItemsSource = icod.Tags;

                if (icod.IsSymbolIcon)
                {
                    CodePointBlock.Visibility = Visibility.Collapsed;
                    CodePointTitleBlock.Visibility = Visibility.Collapsed;
                    TextGlyphBlock.Visibility = Visibility.Collapsed;
                    TextGlyphTitleBlock.Visibility = Visibility.Collapsed;
                    CodeGlyphBlock.Visibility = Visibility.Collapsed;
                    CodeGlyphTitleBlock.Visibility = Visibility.Collapsed;
                    SymbolBlock.Visibility = Visibility.Visible;
                    SymbolTitleBlock.Visibility = Visibility.Visible;

                    SymbolBlock.Text = "Symbol." + icod.Symbol;
                    XAMLCodePresenter.Text = $"<SymbolIcon Symbol=\"{icod.Symbol}\"/>";
                    CSharpCodePresenter.Text = $"var icon = new SymbolIcon(Symbol.{icod.Symbol});";
                }
                else
                {
                    CodePointBlock.Visibility = Visibility.Visible;
                    CodePointTitleBlock.Visibility = Visibility.Visible;
                    TextGlyphBlock.Visibility = Visibility.Visible;
                    TextGlyphTitleBlock.Visibility = Visibility.Visible;
                    CodeGlyphBlock.Visibility = Visibility.Visible;
                    CodeGlyphTitleBlock.Visibility = Visibility.Visible;
                    SymbolBlock.Visibility = Visibility.Collapsed;
                    SymbolTitleBlock.Visibility = Visibility.Collapsed;

                    CodePointBlock.Text = icod.Code;
                    TextGlyphBlock.Text = $"&#x{icod.Code};";
                    CodeGlyphBlock.Text = $"\\u{icod.Code}";
                    XAMLCodePresenter.Text = $"<FontIcon Glyph=\"&#x{icod.Code}\"/>";
                    CSharpCodePresenter.Text = $"FontIcon icon = new FontIcon();\nicon.Glyph = \"\\u{icod.Code}\";";
                }
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Filter(searchBox.Text);
        }

        public void Filter(string search)
        {
            loadingRing.Visibility = Visibility.Visible;
            loadingRing.IsActive = true;

            // Clearing itemssource so user thinks we are doing something
            mainGV.ItemsSource = null;

            string[] filter = search.Split(" ");

            // Spawning a new thread to not have the UI freeze because of our search
            new Thread(async () =>
            {
                var newItems = new List<IconData>();
                foreach (var item in IconDataSource.Icons)
                {
                    var fitsFilter = filter.All(entry => (item.Code?.Contains(entry, StringComparison.CurrentCultureIgnoreCase) ?? false)
                            || item.DisplayName.Contains(entry, StringComparison.CurrentCultureIgnoreCase)
                            || item.Tags.Any(tag => string.IsNullOrEmpty(tag) is false && tag.Contains(entry, StringComparison.CurrentCultureIgnoreCase)));

                    if (fitsFilter)
                    {
                        newItems.Add(item);
                    }
                }

                // Updates to anything UI related (e.g. setting ItemsSource) need to be run on UI thread so queue it through dispatcher
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    mainGV.ItemsSource = newItems;

                    string outputString;
                    var filteredItemsCount = newItems.Count;
                    if (filteredItemsCount > 0)
                    {
                        mainGV.SelectedItem = newItems[0];
                        outputString = filteredItemsCount > 1 ? filteredItemsCount + " icons found." : "1 icon found.";
                    }
                    else
                    {
                        outputString = "No icon found.";
                    }

                    UIHelper.AnnounceActionForAccessibility(searchBox, outputString, "AutoSuggestBoxNumberIconsFoundId");

                    loadingRing.Visibility = Visibility.Collapsed;
                    loadingRing.IsActive = false;
                });
            }).Start();
        }

        private void searchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Filter(args.QueryText);
        }

        private void TagsItemsControl_ItemClick(object sender, ItemClickEventArgs e)
        {
            searchBox.Text = e.ClickedItem.ToString() ?? string.Empty;
        }

        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            searchBox.Text = "CircleRing";
        }

        private void Hyperlink_Click_1(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            searchBox.Text = "CircleFill";
        }

        private void themeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            rootGrid.RequestedTheme = MainPage.Current?.ActualTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
        }

        private void themeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            rootGrid.RequestedTheme = MainPage.Current?.ActualTheme != ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
        }
    }

    public partial class IconTemplateSelector : DataTemplateSelector
    {
#pragma warning disable CS8618
        public DataTemplate FontTemplate { get; set; }
        public DataTemplate SymbolTemplate { get; set; }
#pragma warning restore

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is IconData id ? id.IsSymbolIcon ? SymbolTemplate : FontTemplate : throw new ArgumentException("Invalid type.", nameof(item));
        }
    }
}
