using System;
using System.Collections.Generic;
using System.Linq;
using UWPGallery.DataModel;
using UWPGallery.SamplePages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPGallery.ControlPages
{
    public sealed partial class AutoSuggestBoxPage : Page
    {
        private List<string> Cats = new List<string>()
        {
            "Abyssinian",
            "Aegean",
            "American Bobtail",
            "American Curl",
            "American Ringtail",
            "American Shorthair",
            "American Wirehair",
            "Aphrodite Giant",
            "Arabian Mau",
            "Asian cat",
            "Asian Semi-longhair",
            "Australian Mist",
            "Balinese",
            "Bambino",
            "Bengal",
            "Birman",
            "Brazilian Shorthair",
            "British Longhair",
            "British Shorthair",
            "Burmese",
            "Burmilla",
            "California Spangled",
            "Chantilly-Tiffany",
            "Chartreux",
            "Chausie",
            "Colorpoint Shorthair",
            "Cornish Rex",
            "Cymric",
            "Cyprus",
            "Devon Rex",
            "Donskoy",
            "Dragon Li",
            "Dwelf",
            "Egyptian Mau",
            "European Shorthair",
            "Exotic Shorthair",
            "Foldex",
            "German Rex",
            "Havana Brown",
            "Highlander",
            "Himalayan",
            "Japanese Bobtail",
            "Javanese",
            "Kanaani",
            "Khao Manee",
            "Kinkalow",
            "Korat",
            "Korean Bobtail",
            "Korn Ja",
            "Kurilian Bobtail",
            "Lambkin",
            "LaPerm",
            "Lykoi",
            "Maine Coon",
            "Manx",
            "Mekong Bobtail",
            "Minskin",
            "Napoleon",
            "Munchkin",
            "Nebelung",
            "Norwegian Forest Cat",
            "Ocicat",
            "Ojos Azules",
            "Oregon Rex",
            "Persian (modern)",
            "Persian (traditional)",
            "Peterbald",
            "Pixie-bob",
            "Ragamuffin",
            "Ragdoll",
            "Raas",
            "Russian Blue",
            "Russian White",
            "Sam Sawet",
            "Savannah",
            "Scottish Fold",
            "Selkirk Rex",
            "Serengeti",
            "Serrade Petit",
            "Siamese",
            "Siberian or´Siberian Forest Cat",
            "Singapura",
            "Snowshoe",
            "Sokoke",
            "Somali",
            "Sphynx",
            "Suphalak",
            "Thai",
            "Thai Lilac",
            "Tonkinese",
            "Toyger",
            "Turkish Angora",
            "Turkish Van",
            "Turkish Vankedisi",
            "Ukrainian Levkoy",
            "Wila Krungthep",
            "York Chocolate"
        };

        public AutoSuggestBoxPage()
        {
            InitializeComponent();
        }


        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Since selecting an item will also change the text,
            // only listen to changes caused by user entering text.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                var splitText = sender.Text.ToLower().Split(' ');
                foreach (var cat in Cats)
                {
                    var found = splitText.All((key) =>
                    {
                        return cat.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(cat);
                    }
                }
                if (suitableItems.Count == 0)
                {
                    suitableItems.Add("No results found");
                }
                sender.ItemsSource = suitableItems;
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var f = new Flyout();
            f.Content = new TextBlock
            {
                Style = (Style)Application.Current.Resources["BaseTextBlockStyle"],
                Text = "You chose: " + args.SelectedItem.ToString()
            };

            f.ShowAt(sender);
        }

        /// <summary>
        /// This event gets fired anytime the text in the TextBox gets updated.
        /// It is recommended to check the reason for the text changing by checking against args.Reason
        /// </summary>
        /// <param name="sender">The AutoSuggestBox whose text got changed.</param>
        /// <param name="args">The event arguments.</param>
        private async void Control2_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // We only want to get results when it was a user typing,
            // otherwise we assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = await GallerySearchManager.SearchGallery(sender.Text);

                if (suggestions.Items.Count > 0)
                    sender.ItemsSource = sender == Control2 ? suggestions.Items.Select(x => x.Title).ToArray() : suggestions.Items;
                else
                    sender.ItemsSource = sender == Control2 ? new string[] { "No results found" } : null;
            }
        }

        /// <summary>
        /// This event gets fired when:
        ///     * a user presses Enter while focus is in the TextBox
        ///     * a user clicks or tabs to and invokes the query button (defined using the QueryIcon API)
        ///     * a user presses selects (clicks/taps/presses Enter) a suggestion
        /// </summary>
        /// <param name="sender">The AutoSuggestBox that fired the event.</param>
        /// <param name="args">The args contain the QueryText, which is the text in the TextBox,
        /// and also ChosenSuggestion, which is only non-null when a user selects an item in the list.</param>
        private async void Control2_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender == Control2)
            {
                if (args.ChosenSuggestion is string item)
                {
                    var suggest = await GallerySearchManager.SearchGallery(item);

                    if (suggest.Items.Any())
                    {
                        // User selected an item, take an action
                        SelectControl(sender, suggest.Items.First());
                    }
                }
                else if (!string.IsNullOrEmpty(args.QueryText))
                {
                    var suggestions = await GallerySearchManager.SearchGallery(args.QueryText);
                    if (suggestions.Items.Count > 0)
                    {
                        if (suggestions.Items.FirstOrDefault() is ControlInfoDataItem c)
                        {
                            SelectControl(sender, c);
                        }
                    }
                }
            }
            else if (args.ChosenSuggestion is ControlInfoDataItem control)
            {
                SelectControl(sender, control);
            }
        }

        /// <summary>
        /// This event gets fired as the user keys through the list, or taps on a suggestion.
        /// This allows you to change the text in the TextBox to reflect the item in the list.
        /// Alternatively you can use TextMemberPath.
        /// </summary>
        /// <param name="sender">The AutoSuggestBox that fired the event.</param>
        /// <param name="args">The args contain SelectedItem, which contains the data item of the item that is currently highlighted.</param>
        private void Control2_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            //Don't autocomplete the TextBox when we are showing "no results"
            if (args.SelectedItem is ControlInfoDataItem control)
            {
                sender.Text = control.Title;
            }
            else if (args.SelectedItem is string s)
            {
                if (!s.Equals("No results found", StringComparison.OrdinalIgnoreCase))
                {
                    sender.Text = s;
                }
            }
        }

        private void SelectControl(AutoSuggestBox sender, ControlInfoDataItem control)
        {
            if (control != null)
            {
                if (sender == Control2)
                {
                    ControlDetails.Visibility = Visibility.Visible;

                    ControlImage.Glyph = control.IconGlyph;

                    ControlTitle.Text = control.Title;
                    ControlSubtitle.Text = control.Subtitle;
                }
                else
                {
                    ControlDetails2.Visibility = Visibility.Visible;

                    ControlImage2.Glyph = control.IconGlyph;

                    ControlTitle2.Text = control.Title;
                    ControlSubtitle2.Text = control.Subtitle;
                }
            }
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
    }
}
