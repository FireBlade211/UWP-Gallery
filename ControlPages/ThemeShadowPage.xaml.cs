using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using System.Numerics;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class ThemeShadowPage : Page
    {
        public ThemeShadowPage()
        {
            InitializeComponent();
        }

        private void ShadowRect_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8)
                && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
            {
                var shadow = new ThemeShadow();
                shadow.Receivers.Add(ShadowCastGrid);
                ShadowRect.Shadow = shadow;
            }
        }
    }

    public partial class ZTranslationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Vector3(0, 0, float.CreateChecked((double)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((Vector3)value).Z;
        }
    }
}
