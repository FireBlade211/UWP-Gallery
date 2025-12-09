using System;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RevealPage : Page
    {
        //public Thickness RevealFocusMargin { get; set; }
        //public Thickness RevealFocusThickness1 { get; set; }
        //public Thickness RevealFocusThickness2 { get; set; }

        //public static readonly DependencyProperty RevealFocusMarginProperty = DependencyProperty.Register("RevealFocusMargin",
        //    typeof(Thickness),
        //    typeof(RevealPage),
        //    new PropertyMetadata(null));

        //public static readonly DependencyProperty RevealFocusThickness1Property = DependencyProperty.Register("RevealFocusThickness1",
        //    typeof(Thickness),
        //    typeof(RevealPage),
        //    new PropertyMetadata(null));

        //public static readonly DependencyProperty RevealFocusThickness2Property = DependencyProperty.Register("RevealFocusThickness2",
        //    typeof(Thickness),
        //    typeof(RevealPage),
        //    new PropertyMetadata(null));

        public RevealPage()
        {
            InitializeComponent();
        }


        //private void confirmColor_Click(object sender, RoutedEventArgs e)
        //{
        //    primaryColorPickerButton.Background = new SolidColorBrush(myPrimaryColorPicker.Color);
        //    secondaryColorPickerButton.Background = new SolidColorBrush(mySecondaryColorPicker.Color);

        //    primaryColorPickerButton.Flyout.Hide();
        //    secondaryColorPickerButton.Flyout.Hide();
        //}

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

        //private void Example2_ActualThemeChanged(FrameworkElement sender, object args)
        //{
        //    if (Example2.ActualTheme == ElementTheme.Light)
        //    {
        //        myPrimaryColorPicker.Color = Colors.Black;
        //        mySecondaryColorPicker.Color = Colors.White;
        //    }
        //    else if (Example2.ActualTheme == ElementTheme.Dark)
        //    {
        //        myPrimaryColorPicker.Color = Colors.White;
        //        mySecondaryColorPicker.Color = Colors.Black;
        //    }
        //}

        //private void MoveFocusBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    exampleButton.Focus(FocusState.Keyboard);
        //}

        //private void MarginResetButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Draw the focus visuals at the edge of the control
        //    // A negative FocusVisualMargin outsets the focus visual. A positive one insets the focus visual
        //    // We don't know the exact perfect value to use so compute an average
        //    var value = -1 * Math.Round((((primaryLeftSlider.Value + primaryRightSlider.Value + primaryTopSlider.Value + primaryBottomSlider.Value) / 4)
        //        + ((secondaryLeftSlider.Value + secondaryRightSlider.Value + secondaryTopSlider.Value + secondaryBottomSlider.Value) / 4)));

        //    marginBottomSlider.Value = value;
        //    marginLeftSlider.Value = value;
        //    marginRightSlider.Value = value;
        //    marginTopSlider.Value = value;
        //}

        //private void primaryLeftSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness1 = new Thickness(
        //        e.NewValue,
        //        RevealFocusThickness1.Top,
        //        RevealFocusThickness1.Right,
        //        RevealFocusThickness1.Bottom);
        //}

        //private void primaryRightSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness1 = new Thickness(
        //        RevealFocusThickness1.Left,
        //        RevealFocusThickness1.Top,
        //        e.NewValue,
        //        RevealFocusThickness1.Bottom);
        //}

        //private void primaryTopSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness1 = new Thickness(
        //        RevealFocusThickness1.Left,
        //        e.NewValue,
        //        RevealFocusThickness1.Right,
        //        RevealFocusThickness1.Bottom);
        //}

        //private void primaryBottomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness1 = new Thickness(
        //        RevealFocusThickness1.Left,
        //        RevealFocusThickness1.Top,
        //        RevealFocusThickness1.Right,
        //        e.NewValue);
        //}

        //private void secondaryLeftSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness2 = new Thickness(
        //        e.NewValue,
        //        RevealFocusThickness2.Top,
        //        RevealFocusThickness2.Right,
        //        RevealFocusThickness2.Bottom);
        //}

        //private void secondaryRightSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness2 = new Thickness(
        //        RevealFocusThickness2.Left,
        //        RevealFocusThickness2.Top,
        //        e.NewValue,
        //        RevealFocusThickness2.Bottom);
        //}

        //private void secondaryTopSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness2 = new Thickness(
        //        RevealFocusThickness2.Left,
        //        e.NewValue,
        //        RevealFocusThickness2.Right,
        //        RevealFocusThickness2.Bottom);
        //}

        //private void secondaryBottomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusThickness2 = new Thickness(
        //        RevealFocusThickness2.Left,
        //        RevealFocusThickness2.Top,
        //        RevealFocusThickness2.Right,
        //        e.NewValue);
        //}

        //private void marginTopSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusMargin = new Thickness(
        //        RevealFocusMargin.Left,
        //        e.NewValue,
        //        RevealFocusMargin.Right,
        //        RevealFocusMargin.Bottom);
        //}

        //private void marginLeftSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusMargin = new Thickness(
        //         e.NewValue,
        //         RevealFocusMargin.Top,
        //         RevealFocusMargin.Right,
        //         RevealFocusMargin.Bottom);
        //}

        //private void marginRightSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusMargin = new Thickness(
        //         RevealFocusMargin.Left,
        //         RevealFocusMargin.Top,
        //         e.NewValue,
        //         RevealFocusMargin.Bottom);
        //}

        //private void marginBottomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    RevealFocusMargin = new Thickness(
        //         RevealFocusMargin.Left,
        //         RevealFocusMargin.Top,
        //         RevealFocusMargin.Right,
        //         e.NewValue);
        //}
    }

    public partial class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return null!;
        }
    }
}