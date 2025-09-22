using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class XamlUICommandPage : Page
    {
        private int TriggerCount = 0;

        public XamlUICommandPage()
        {
            InitializeComponent();
        }

        private void XamlUICommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            TriggerCount++;
            CommandTextBlock.Text = $"You triggered the command {TriggerCount} times!";
        }
    }
}
