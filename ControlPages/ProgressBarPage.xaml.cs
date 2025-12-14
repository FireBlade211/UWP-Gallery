using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressBarPage : Page
    {
        public ProgressBarPage()
        {
            this.InitializeComponent();
        }

        private void holdRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            holdProgressBar.Value = (holdProgressBar.Value + 1) % (100 + 1);
        }
    }
}
