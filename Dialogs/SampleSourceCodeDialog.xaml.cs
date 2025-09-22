using UWPGallery.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.Dialogs
{
    public sealed partial class SampleSourceCodeDialog : ContentDialog
    {
        public string? CSharpSource
        {
            get => (string?)GetValue(CSharpSourceProperty);
            set => SetValue(CSharpSourceProperty, value);
        }

        public static readonly DependencyProperty CSharpSourceProperty = DependencyProperty.Register(
            "CSharpSource",
            typeof(string),
            typeof(GallerySample),
            null);

        public SampleSourceCodeDialog()
        {
            InitializeComponent();
        }
    }
}
