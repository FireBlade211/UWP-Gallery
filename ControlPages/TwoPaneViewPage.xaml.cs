using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPGallery.ControlPages
{
    public sealed partial class TwoPaneViewPage : Page
    {
        public TwoPaneViewPage()
        {
            InitializeComponent();
        }

        private void panePriorityToggle_Toggled(object sender, RoutedEventArgs e)
        {
            singlePaneView.PanePriority = panePriorityToggle.IsOn ? TwoPaneViewPriority.Pane2 : TwoPaneViewPriority.Pane1;
        }

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            // Remove details content from it's parent panel.
            ((Panel)DetailsContent.Parent).Children.Remove(DetailsContent);
            // Set Normal visual state.
            VisualStateManager.GoToState(this, "Normal", true);
            pictureInfoSample.MinHeight = 1400;

            // Single pane
            if (sender.Mode == TwoPaneViewMode.SinglePane)
            {
                // Add the details content to Pane1.
                Pane1StackPanel.Children.Add(DetailsContent);
            }
            // Dual pane.
            else
            {
                // Put details content in Pane2.
                Pane2Root.Children.Add(DetailsContent);

                // If also in Wide mode, set Wide visual state
                // to constrain the width of the image to 2*.
                if (sender.Mode == TwoPaneViewMode.Wide)
                {
                    VisualStateManager.GoToState(this, "Wide", true);
                    pictureInfoSample.MinHeight = 750;
                }
            }
        }
    }
}
