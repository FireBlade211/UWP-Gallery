using Windows.ApplicationModel.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using System;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPanePage : Page
    {
        public SearchPanePage()
        {
            InitializeComponent();
        }

        private void OpenButtonSample1_Click(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().Show();
        }

        private async void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("uwp-gallery:///page/autosuggestbox"));
        }

        private async void Hyperlink_Click_1(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("uwp-gallery:///page/searchbox"));
        }
    }
}
