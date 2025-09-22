using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ButtonPage : Page
    {
        private int simpleButtonClickCount = 0;

        public ButtonPage()
        {
            InitializeComponent();
        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            simpleButtonClickCount++;

            SimpleButtonClickCounter.Text = $"Clicked: {simpleButtonClickCount} times";
        }
    }
}
