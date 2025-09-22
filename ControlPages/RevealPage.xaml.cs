using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RevealPage : Page
    {
        public RevealPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            switch (Application.Current.FocusVisualKind)
            {
                case FocusVisualKind.HighVisibility:
                    HighVisibilityRadio.IsChecked = true;
                    break;
                case FocusVisualKind.Reveal:
                    RevealFocusRadio.IsChecked = true;
                    break;
                case FocusVisualKind.DottedLine:
                    DottedLineRadio.IsChecked = true;
                    break;
            }
        }

        private void HighVisibilityRadio_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.FocusVisualKind = FocusVisualKind.HighVisibility;
        }

        private void RevealFocusRadio_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.FocusVisualKind = FocusVisualKind.Reveal;
        }

        private void DottedLineRadio_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.FocusVisualKind = FocusVisualKind.DottedLine;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button1.Focus(FocusState.Keyboard);
        }
    }
}
