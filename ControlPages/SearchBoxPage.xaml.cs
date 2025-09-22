using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using System;
using System.Threading.Tasks;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchBoxPage : Page
    {
        public SearchBoxPage()
        {
            this.InitializeComponent();
        }

        private async void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("uwp-gallery:///page/autosuggestbox"));
        }
    }
}
