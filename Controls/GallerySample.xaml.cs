using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using UWPGallery.Dialogs;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPGallery.Controls
{
    [ContentProperty(Name = "Sample")]
    public sealed partial class GallerySample : UserControl
    {
        public string HeaderTitle
        {
            get => (string)GetValue(HeaderTitleProperty);
            set => SetValue(HeaderTitleProperty, value);
        }

        public static readonly DependencyProperty HeaderTitleProperty = DependencyProperty.Register(
            "HeaderTitle",
            typeof(string),
            typeof(GallerySample),
            null);

        public UIElement? Sample
        {
            get => (UIElement?)GetValue(SampleProperty);
            set => SetValue(SampleProperty, value);
        }

        public static readonly DependencyProperty SampleProperty = DependencyProperty.Register(
            "Sample",
            typeof(UIElement),
            typeof(GallerySample),
            null);

        public string? XamlSource
        {
            get => (string?)GetValue(XamlSourceProperty);
            set => SetValue(XamlSourceProperty, value);
        }

        public static readonly DependencyProperty XamlSourceProperty = DependencyProperty.Register(
            "XamlSource",
            typeof(string),
            typeof(GallerySample),
            null);

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

        public string? UrlNameOverride
        {
            get => (string?)GetValue(UrlNameOverrideProperty);
            set => SetValue(UrlNameOverrideProperty, value);
        }

        public static readonly DependencyProperty UrlNameOverrideProperty = DependencyProperty.Register(
            "UrlNameOverride",
            typeof(string),
            typeof(GallerySample),
            new PropertyMetadata(null));

        public string SampleUrl => UrlNameOverride ?? HeaderTitle.Replace(' ', '-');
        public FlyoutBase? OptionsFlyout { get; set; }

        public static readonly DependencyProperty OptionsFlyoutProperty = DependencyProperty.Register(
            "OptionsFlyout",
            typeof(FlyoutBase),
            typeof(GallerySample),
            new PropertyMetadata(null));

        public GallerySample()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateThemeAppBarButtonIcon();
        }

        private void UpdateThemeAppBarButtonIcon()
        {
            Debug.WriteLine("Update icon ran!");
            ThemeAppBarButton.Icon = SampleGrid.ActualTheme == ElementTheme.Light ? new FontIcon
            {
                Glyph = "\uE793",
                FontFamily = new FontFamily("Segoe MDL2 Assets")
            } : new FontIcon
            {
                Glyph = "\uE708",
                FontFamily = new FontFamily("Segoe MDL2 Assets")
            };
        }

        private void ThemeAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateThemeAppBarButtonIcon();

            SampleGrid.RequestedTheme = SampleGrid.ActualTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
        }

        private void RtlAppBarButton_Checked(object sender, RoutedEventArgs e)
        {
            SampleGrid.FlowDirection = RtlAppBarButton.IsChecked == true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private async void SourceAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SampleSourceCodeDialog();

            if (XamlSource == null)
                dlg.SourcePivot.Items.Remove(dlg.SourcePivotXamlItem);
            else
                if (!dlg.SourcePivot.Items.Contains(dlg.SourcePivotXamlItem))
                dlg.SourcePivot.Items.Add(dlg.SourcePivotXamlItem);

            if (CSharpSource == null)
                dlg.SourcePivot.Items.Remove(dlg.SourcePivotCSItem);
            else
                if (!dlg.SourcePivot.Items.Contains(dlg.SourcePivotCSItem))
                dlg.SourcePivot.Items.Add(dlg.SourcePivotCSItem);

            dlg.XamlRichEdit.IsReadOnly = false;
            dlg.XamlRichEdit.TextDocument.SetText(TextSetOptions.None, XamlSource ?? string.Empty);

            string cachedRtf = string.Empty;

            dlg.XamlRichEdit.LostFocus += (s, e) =>
            {
                dlg.XamlRichEdit.IsReadOnly = false;
                dlg.XamlRichEdit.Document.SetText(TextSetOptions.FormatRtf, cachedRtf);
                dlg.XamlRichEdit.FontFamily = new FontFamily("Consolas,Courier New,Segoe UI");
                dlg.XamlRichEdit.IsReadOnly = true;
            };

            var f = new XamlTextFormatter(dlg.XamlRichEdit);
            var theme = await ConfigurationStorageManager.GetAppTheme();
            dlg.RequestedTheme = theme;

            if (theme == ElementTheme.Light || (theme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Light))
            {
                f.ForceTheme = ElementTheme.Light;
            }

            f.ApplyColors();

            dlg.XamlRichEdit.IsReadOnly = true;

            dlg.XamlRichEdit.Document.GetText(TextGetOptions.FormatRtf, out cachedRtf);


            dlg.CSharpSource = CSharpSource;

            dlg.Loaded += (s, e) =>
            {
                dlg.XamlRichEdit.IsReadOnly = false;
                dlg.XamlRichEdit.Document.SetText(TextSetOptions.FormatRtf, cachedRtf);
                dlg.XamlRichEdit.FontFamily = new FontFamily("Consolas,Courier New,Segoe UI");
                dlg.XamlRichEdit.IsReadOnly = true;
            };

            _ = dlg.ShowAsync();
        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CopyLinkButton.Visibility = Visibility.Visible;
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CopyLinkButton.Visibility = Visibility.Collapsed;
        }

        private void CopyLinkButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Click ran!");

            if (MainPage.Current != null)
            {
                if (MainPage.Current.NavBar.SelectedItem is NavigationViewItem nvi)
                {
                    if (nvi.DataContext is ControlInfoDataItem item)
                    {
                        Clipboard.SetContent(ProtocolActivationClipboardHelper.CreateDataPackage(item, this));
                        CopyLinkIcon.Glyph = "\uE73E";

                        ToolTipService.SetToolTip(CopyLinkButton, "Copied!");

                        Task.Delay(1500).ContinueWith(task =>
                        {
                            CopyLinkIcon.Glyph = "\uE71B";

                            ToolTipService.SetToolTip(CopyLinkButton, "Copy link to sample");

                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            // use this as a workaround to hide the flyout arrow
            OptionsFlyout?.ShowAt(sender as FrameworkElement, new FlyoutShowOptions
            {
                Placement = FlyoutPlacementMode.Left
            });
        }
    }

    public partial class NullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value ?? System.Convert.ChangeType(parameter, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class NullableObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException("Converting a bool to an object is impossible. Do not use TwoWay bindings with this converter.");
        }
    }

    public class XamlTextFormatter
    {
        private readonly RichEditBox m_richEditBox;

        public ElementTheme? ForceTheme;

        enum ZoneType
        {
            Unknown,
            OpenTag,
            EndTag,
            TagName,
            PropertyName,
            PropertyValue,
            Whitespace,
            Comment,
            Content
        }

        public XamlTextFormatter(RichEditBox richEditBox)
        {
            m_richEditBox = richEditBox;
            FocusHelper.StartTrackFocusState(m_richEditBox);
        }

        public void ApplyColors()
        {
            var doc = m_richEditBox.Document;
            doc.BeginUndoGroup();

            string rebText;
            doc.GetText(TextGetOptions.None, out rebText);

            var startIndex = 0;
            var currentZoneType = ZoneType.Unknown;
            bool inATag = false;
            for (var currIndex = 0; currIndex < rebText.Length; currIndex++)
            {
                if (char.IsWhiteSpace(rebText[currIndex]))
                {
                    if (currentZoneType != ZoneType.Whitespace)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.Whitespace;
                    }
                    // else already whitespace, so continue
                }
                else if (rebText[currIndex] == '<')
                {
                    UpdateZone(startIndex, currIndex, currentZoneType);
                    startIndex = currIndex;
                    if (rebText.Substring(currIndex).StartsWith("<!--"))
                    {
                        currentZoneType = ZoneType.Comment;
                        currIndex += 3;

                        // Look for end of comment
                        var endIndex = rebText.IndexOf("-->", currIndex + 1);
                        if (endIndex >= 0)
                        {
                            currIndex = endIndex + 2;
                        }
                    }
                    else
                    {
                        currentZoneType = ZoneType.OpenTag;
                        inATag = true;
                    }
                }
                else if (rebText[currIndex] == '/')
                {
                    // This could either be "</" at the start a tag like "</StackPanel>", or "/>" at
                    // the end of a tag like "<Button/>".
                    if (currentZoneType != ZoneType.OpenTag && currentZoneType != ZoneType.EndTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.EndTag;
                        inATag = false;
                    }
                    // else already end tag, so continue
                }
                else if (rebText[currIndex] == '>')
                {
                    if (currentZoneType != ZoneType.EndTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.EndTag;
                        inATag = false;
                    }
                    // else already end tag, so continue
                }
                else
                {
                    if (currentZoneType == ZoneType.OpenTag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.TagName;
                    }
                    else if (currentZoneType == ZoneType.PropertyName && rebText[currIndex] == '=')
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.PropertyValue;

                        // Look for the end of the value
                        if (currIndex < rebText.Length - 2 && rebText[currIndex + 1] == '"')
                        {
                            var endIndex = rebText.IndexOf("\"", currIndex + 2);
                            if (endIndex >= 0)
                            {
                                currIndex = endIndex;
                            }
                        }
                    }
                    else if (currentZoneType != ZoneType.TagName && currentZoneType != ZoneType.PropertyName
                         && currentZoneType != ZoneType.PropertyValue && inATag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.PropertyName;
                    }
                    else if (!inATag)
                    {
                        UpdateZone(startIndex, currIndex, currentZoneType);
                        startIndex = currIndex;
                        currentZoneType = ZoneType.Content;
                    }
                    // else already ...other..., so continue
                }
            }
            doc.EndUndoGroup();
        }

        // endIndexExclusive is the index just after the zone.
        private void UpdateZone(int startIndex, int endIndexExclusive, ZoneType zoneType)
        {
            if (startIndex >= endIndexExclusive)
            {
                return;
            }

            bool lightTheme = (ForceTheme ?? m_richEditBox.ActualTheme) != ElementTheme.Dark;

            var doc = m_richEditBox.Document;
            var range = doc.GetRange(startIndex, endIndexExclusive);
            Color foregroundColor = lightTheme ? Colors.Black : Colors.White;
            switch (zoneType)
            {
                case ZoneType.OpenTag:
                case ZoneType.EndTag:
                    foregroundColor = lightTheme ? Colors.Blue : Colors.Gray;
                    break;

                case ZoneType.TagName:
                    foregroundColor = lightTheme ? Colors.Brown : Colors.White;
                    break;

                case ZoneType.PropertyName:
                    foregroundColor = lightTheme ? Colors.Red : Colors.LightSkyBlue;
                    break;

                case ZoneType.PropertyValue:
                    foregroundColor = lightTheme ? Colors.Blue : Colors.DodgerBlue;
                    break;

                case ZoneType.Whitespace:
                case ZoneType.Content:
                    foregroundColor = lightTheme ? Colors.Black : Colors.White;
                    break;

                case ZoneType.Comment:
                    foregroundColor = lightTheme ? Colors.Green : Colors.LimeGreen;
                    break;
            }

            range.CharacterFormat.ForegroundColor = foregroundColor;
        }
    }
}
