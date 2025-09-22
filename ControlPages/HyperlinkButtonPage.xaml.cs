using UWPGallery.DataModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HyperlinkButtonPage : Page
    {
        public HyperlinkButtonPage()
        {
            this.InitializeComponent();
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ControlInfoDataSource.Instance.GetGroupsAsync();

            var cidi = await ControlInfoDataSource.GetItemAsync("HyperlinkButton");

            if (cidi != null)
            {
                App.ShareUIData = cidi;

                DataTransferManager.ShowShareUI();
            }
        }

        private async void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("uwp-gallery:///page/button"));
        }
    }
}
