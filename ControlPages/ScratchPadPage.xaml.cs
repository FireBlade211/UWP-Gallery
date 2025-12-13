using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UWPGallery.Controls;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Core;
using Windows.System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Shapes;

namespace UWPGallery.ControlPages
{
    public sealed partial class ScratchPadPage : Page
    {
        public ScratchPadPage()
        {
            InitializeComponent();

            _ = Init();
        }

        private async Task Init()
        {
            var xamlStr = await ConfigurationStorageManager.GetLastScratchPadXaml();

            // If there is no stored XAML, load the default.
            if (xamlStr == null || xamlStr.Trim().Length == 0)
            {
                xamlStr = DefaultScratchPadXAML;
            }

            m_oldText = xamlStr;
            textbox.TextDocument.SetText(TextSetOptions.None, m_oldText);

            var formatter = new XamlTextFormatter(textbox);
            formatter.ApplyColors();

            // Provide some initial instruction in the content area.
            SetEmptyScratchPadContent();
        }

        private void SetEmptyScratchPadContent()
        {
            scratchPad.Content = new TextBlock()
            {
                Text = "Click the Load button to load the XAML content in the box below.",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static readonly string DefaultScratchPadXAML = @"<StackPanel Padding=""3"">
    <!-- Note: {x:Bind} is not supported in the Scratch Pad. -->
    <TextBlock>This is a sample TextBlock.</TextBlock>
    <Button Content=""Click me!""/>
</StackPanel>";

        private async void ResetToDefaultClick(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Are you sure you want to reset?";
            dialog.Content = "Resetting to the default content will replace your current content. Are you sure you want to reset?";
            dialog.PrimaryButtonText = "Reset";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                m_oldText = DefaultScratchPadXAML;
                textbox.TextDocument.SetText(TextSetOptions.None, m_oldText);
                if (colorCodeToggle.IsChecked == true)
                {
                    var formatter = new XamlTextFormatter(textbox);
                    formatter.ApplyColors();
                }

                SetEmptyScratchPadContent();
                loadStatus.Text = "";
            }
        }

        private string AddXmlNamespace(string xml)
        {
            xml = xml.Trim();

            char[] chars = { ' ', '/', '>' };
            var insertIndex = xml.IndexOfAny(chars);
            if (insertIndex < 0)
            {
                throw new ArgumentException("No end tag.");
            }

            xml = xml.Substring(0, insertIndex) + " xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' " + xml.Substring(insertIndex);
            return xml;
        }

        [GeneratedRegex("(?s)(?<!<!--(?:(?!-->)[\\s\\S])*?)\\{x:Bind[^}]*\\}")]
        private static partial Regex XBindRegex();

        private List<ScratchPadSpecialControlSetupInfo> GetSpecialSetupInfo(UIElement c)
        {
            var infos = new List<ScratchPadSpecialControlSetupInfo>();

            switch (c)
            {
                case Grid g:
                    foreach (var child in g.Children)
                    {
                        var i = GetSpecialSetupInfo(child);

                        infos.AddRange(i);
                    }
                    break;
                case StackPanel s:
                    foreach (var child in s.Children)
                    {
                        var i = GetSpecialSetupInfo(child);

                        infos.AddRange(i);
                    }
                    break;
                case ContentControl conctrl:
                    if (conctrl.Content is UIElement uie)
                    {
                        infos.AddRange(GetSpecialSetupInfo(uie));
                    }
                    break;
                case InkCanvas i:
                    var info = new ScratchPadSpecialControlSetupInfo();
                    info.SetupName = "Allow all input types for InkCanvas.InkPresenter";
                    info.SetupAction = (UIElement root) =>
                    {
                        i.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch;
                    };

                    infos.Add(info);
                    break;
                case InkToolbar it:
                    var inf = new ScratchPadSpecialControlSetupInfo();
                    inf.SetupName = "Find and associate first available InkCanvas to InkToolbar.TargetInkCanvas";
                    inf.SetupAction = (UIElement root) =>
                    {
                        void scanICRecursive(UIElement e)
                        {
                            switch (e)
                            {
                                case Grid gg:
                                    foreach (var cc in gg.Children)
                                    {
                                        if (cc is InkCanvas ic)
                                        {
                                            it.TargetInkCanvas = ic;
                                        }
                                        else
                                        {
                                            scanICRecursive(cc);
                                        }
                                    }
                                    break;
                                case StackPanel ss:
                                    foreach (var cc in ss.Children)
                                    {
                                        if (cc is InkCanvas ic)
                                        {
                                            it.TargetInkCanvas = ic;
                                        }
                                        else
                                        {
                                            scanICRecursive(cc);
                                        }
                                    }
                                    break;
                                case InkCanvas ic:
                                    it.TargetInkCanvas = ic;
                                    break;
                                case ContentControl cc:
                                    if (cc.Content is InkCanvas iic)
                                    {
                                        it.TargetInkCanvas = iic;
                                    }
                                    break;
                                case Border b:
                                    if (b.Child is InkCanvas iicc)
                                    {
                                        it.TargetInkCanvas = iicc;
                                    }
                                    break;
                            }
                        }

                        if (root is Grid g)
                        {
                            var canvas = g.Children.OfType<InkCanvas>().FirstOrDefault();

                            if (canvas != null)
                            {
                                it.TargetInkCanvas = canvas;
                            }
                            else
                            {
                                foreach (var child in g.Children)
                                {
                                    scanICRecursive(child);
                                }
                            }
                        }
                        else if (root is StackPanel s)
                        {
                            var canvas = s.Children.OfType<InkCanvas>().FirstOrDefault();

                            if (canvas != null)
                            {
                                it.TargetInkCanvas = canvas;
                            }
                            else
                            {
                                foreach (var child in s.Children)
                                {
                                    scanICRecursive(child);
                                }
                            }
                        }
                    };

                    infos.Add(inf);
                    break;
                case Border b:
                    if (b.Child is UIElement e)
                    {
                        infos.AddRange(GetSpecialSetupInfo(e));
                    }
                    break;
            }

            return infos;
        }

        private async Task LoadContent()
        {
            string newText;
            textbox.TextDocument.GetText(TextGetOptions.None, out newText);
            //System.Diagnostics.Debug.WriteLine("new text: " + newText);

            await ConfigurationStorageManager.SetLastScratchPadXaml(newText);

            try
            {
                loadStatus.Text = ""; // Clear the log before loading

                var xml = AddXmlNamespace(newText);
                var theme = await ConfigurationStorageManager.GetAppTheme();

                // Strip out x:Bind
                var stripped = XBindRegex().Replace(xml, string (Match m) => new string(' ', m.Value.Length));

                if (!stripped.Equals(xml, StringComparison.OrdinalIgnoreCase))
                {
                    var result = await new ContentDialog
                    {
                        Title = "Unsupported Bindings",
                        Content = "x:Bind bindings have been detected in the XAML content. These bindings are not supported in the Scratch Pad." +
                        " Do you want to continue loading the content?",
                        PrimaryButtonText = "Continue",
                        SecondaryButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                        RequestedTheme = theme
                    }.ShowAsync();

                    if (result != ContentDialogResult.Primary)
                    {
                        loadStatus.Text = "Load cancelled by user.\n" + loadStatus.Text;
                        return;
                    }
                }

                loadStatus.Text = string.Empty;

                var element = (UIElement)XamlReader.Load(xml);
                var infos = GetSpecialSetupInfo(element);

                if (infos.Any())
                {
                    var result = await new ContentDialog
                    {
                        Title = "Special Setup Detected",
                        Content = "There were special setups detected for some controls. This means that UWP Gallery can configure some properties of these controls for you," +
                        " even if they aren't configurable in XAML. Do you want to apply these special setups?\n\nThe following special setups were detected:\n- " +
                        string.Join("\n- ", infos),
                        PrimaryButtonText = "Apply special setups",
                        SecondaryButtonText = "Keep original content",
                        DefaultButton = ContentDialogButton.Primary,
                        RequestedTheme = theme
                    }.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        foreach (var info in infos)
                        {
                            info.SetupAction(element);
                            loadStatus.Text = $"Applied special setup: {info.SetupName}\n{loadStatus.Text}";
                        }
                    }
                }

                scratchPad.Content = element;
                loadStatus.Text += "Load successful.";
            }
            catch (Exception ex)
            {
                loadStatus.Text = ex.Message + "\n" + loadStatus.Text;
            }
            loadStatus.Opacity = 1.0;
        }

        private async Task LoadContentAndApplyFormatting()
        {
            await LoadContent();

            m_lastChangeFromTyping = false;
        }

        private void InsertTextboxText(string str, bool setCursorAfterInsertedStr)
        {
            var selectionStart = textbox.TextDocument.Selection.StartPosition;
            var range = textbox.TextDocument.GetRange(selectionStart, selectionStart);
            m_lastChangeFromTyping = false;
            range.Text = str;
            textbox.TextDocument.Selection.StartPosition = selectionStart + (setCursorAfterInsertedStr ? str.Length : 0);
        }

        private string GetTextboxTextPreviousLine()
        {
            string newText;
            textbox.TextDocument.GetText(TextGetOptions.None, out newText);
            var selectionIndex = textbox.TextDocument.Selection.StartPosition;
            if (selectionIndex > 0)
            {
                char[] eolChars = { '\r', '\n' };
                var endOfLineIndex = newText.LastIndexOfAny(eolChars, selectionIndex - 1);
                if (endOfLineIndex > 0)
                {
                    var startOfLineIndex = newText.LastIndexOfAny(eolChars, endOfLineIndex - 1) + 1;
                    return newText.Substring(startOfLineIndex, endOfLineIndex - startOfLineIndex);
                }
            }
            return "";
        }

        private void HandleEnter()
        {
            if (prettyPrintToggle.IsChecked == true)
            {
                textbox.TextDocument.BeginUndoGroup();
                string previousLine = GetTextboxTextPreviousLine();
                string indentStr = previousLine.Substring(0, previousLine.Length - previousLine.TrimStart().Length);
                // TODO: Indent further if this is the start of a child tag or property.
                InsertTextboxText(indentStr, true);

                // If this looks like the start of content area in a tag, further indent and put the end tag on a new line.
                var selectionStart = textbox.TextDocument.Selection.StartPosition;
                var range = textbox.TextDocument.GetRange(selectionStart, selectionStart + 2);
                if (range.Text == "</")
                {
                    InsertTextboxText("    ", true);
                    selectionStart = textbox.TextDocument.Selection.StartPosition;
                    range = textbox.TextDocument.GetRange(selectionStart, selectionStart);
                    range.Text = "\n" + indentStr;
                }
                textbox.TextDocument.EndUndoGroup();
            }
        }

        private void textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            m_lastChangeFromTyping = true;

            if (prettyPrintToggle.IsChecked == true)
            {
                switch (e.Key)
                {
                    case VirtualKey.Tab:
                        if (textbox.TextDocument.Selection.Length > 1)
                        {
                            var isShiftKeyDown = ((int)CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift) &
                                (int)CoreVirtualKeyStates.Down) == (int)CoreVirtualKeyStates.Down;
                            string text;
                            textbox.TextDocument.GetText(TextGetOptions.None, out text);

                            var selectionStart = textbox.TextDocument.Selection.StartPosition;
                            var selectionEnd = selectionStart + textbox.TextDocument.Selection.Length;
                            char[] eolChars = { '\r', '\n' };
                            var startOfLineIndex = text.LastIndexOfAny(eolChars, selectionStart) + 1;
                            if (startOfLineIndex >= 0)
                            {
                                var range = textbox.TextDocument.GetRange(startOfLineIndex, startOfLineIndex);

                                if (!isShiftKeyDown) // Indent
                                {
                                    range.Text = "    ";
                                    selectionStart += 4;
                                    selectionEnd += 4;

                                    range.Move(TextRangeUnit.Paragraph, 1);
                                    while (range.StartPosition < selectionEnd)
                                    {
                                        range.Text = "    ";
                                        selectionEnd += 4;
                                        range.Move(TextRangeUnit.Paragraph, 1);
                                    }
                                }
                                else // Unindent
                                {
                                    bool firstLine = true;
                                    while (range.StartPosition < selectionEnd)
                                    {
                                        range.MoveEnd(TextRangeUnit.Character, 4);
                                        var numWhitespace = range.Text.Count(char.IsWhiteSpace);
                                        range = textbox.TextDocument.GetRange(range.StartPosition, range.StartPosition + numWhitespace);
                                        range.Text = "";
                                        if (firstLine)
                                        {
                                            firstLine = false;
                                            selectionStart -= numWhitespace;
                                        }
                                        selectionEnd -= numWhitespace;
                                        range.Move(TextRangeUnit.Paragraph, 1);
                                    }
                                }

                                textbox.TextDocument.Selection.StartPosition = selectionStart;
                                textbox.TextDocument.Selection.EndPosition = selectionEnd;

                                e.Handled = true;
                            }
                        }
                        break;
                }
            }
        }
        private int GetLineStart(ITextDocument doc, int pos)
        {
            var r = doc.GetRange(pos, pos);
            r.Expand(TextRangeUnit.Line);
            return r.StartPosition;
        }

        private async void textbox_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            m_lastChangeFromTyping = true;
            switch (e.Key)
            {
                case VirtualKey.F5:
                    await LoadContentAndApplyFormatting();
                    break;

                case VirtualKey.Enter:
                    HandleEnter();
                    break;

                case VirtualKey.D:
                    if (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
                    {
                        if (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
                        {
                            var doc = textbox.TextDocument;
                            var sel = doc.Selection;

                            int originalCaret = sel.StartPosition;
                            int originalColumn = sel.StartPosition - GetLineStart(doc, sel.StartPosition);

                            // Expand caret range to current line
                            var lineRange = doc.GetRange(sel.StartPosition, sel.StartPosition);
                            lineRange.Expand(TextRangeUnit.Line);

                            // Duplicate line text
                            string text = lineRange.Text;
                            if (!text.EndsWith('\n'))
                                text += "\n";

                            var insertPos = doc.GetRange(lineRange.EndPosition, lineRange.EndPosition);
                            insertPos.Text = text;

                            // Move caret to same column on the duplicated line

                            // After inserting, the duplicated line begins exactly at old lineRange.EndPosition
                            int newLineStart = lineRange.EndPosition;

                            int newCaretTarget = newLineStart + originalColumn;

                            // Clamp to line end to not overshoot
                            int newLineEnd = newLineStart + text.Length;
                            if (newCaretTarget > newLineEnd)
                                newCaretTarget = newLineEnd;

                            doc.Selection.StartPosition = newCaretTarget;
                            doc.Selection.EndPosition = newCaretTarget;

                            if (colorCodeToggle.IsChecked == true)
                            {
                                var formatter = new XamlTextFormatter(textbox);
                                formatter.ForceTheme = ElementTheme.Light;
                                formatter.ApplyColors();
                            }
                        }
                    }
                    break;
            }
        }

        private async void LoadClick(object sender, RoutedEventArgs e)
        {
            await LoadContentAndApplyFormatting();
        }

        bool m_lastChangeFromTyping = false;
        string m_oldText = "";

        private void textbox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (textbox.TextDocument.Selection.Length == 0 && m_lastChangeFromTyping)
            {
                if (loadStatus.Text == "Load successful.")
                {
                    loadStatus.Text = "";
                }
                else
                {
                    loadStatus.Opacity = 0.5; // dim the message which is now old
                }

                if (prettyPrintToggle.IsChecked == true)
                {
                    string newText;
                    textbox.TextDocument.GetText(TextGetOptions.None, out newText);
                    if (newText.Length == m_oldText.Length + 1)
                    {
                        // Added just one character
                        var selectionIndex = textbox.TextDocument.Selection.StartPosition;
                        if (selectionIndex >= 2 && newText[selectionIndex - 1] == '>' && newText[selectionIndex - 2] != '/')
                        {
                            var tagStartIndex = newText.LastIndexOf('<', selectionIndex - 1);
                            // If we found a start tag and there wasn't already a close of that tag
                            if (tagStartIndex >= 0 && newText.LastIndexOf('>', selectionIndex - 2) < tagStartIndex)
                            {
                                var tagName = newText.Substring(tagStartIndex + 1);

                                char[] chars = { ' ', '/', '>', '\t', '\r', '\n' };
                                var nameEndIndex = tagName.IndexOfAny(chars);
                                if (nameEndIndex > 0)
                                {
                                    tagName = tagName = tagName.Substring(0, nameEndIndex);
                                    if (tagName != "!--") // don't add a close tag for a comment
                                    {
                                        InsertTextboxText("</" + tagName + ">", false);
                                    }
                                }
                            }
                        }
                        else if (newText[selectionIndex - 1] == '=' &&
                            (selectionIndex >= newText.Length - 1 || newText[selectionIndex] != '"'))
                        {
                            // Might want to auto-insert quotes for a property. Check if this appears
                            // to be inside a tag and just after a property name.
                            char[] tagChars = { '<', '>' };
                            var lastTagIndex = newText.LastIndexOfAny(tagChars, selectionIndex);
                            if (lastTagIndex >= 0 && newText[lastTagIndex] == '<')
                            {
                                // In a tag. Make sure we aren't in a property value (improper comparison
                                // here is to check if the last quote is proceed by an '=').
                                var quoteIndex = newText.LastIndexOf('"', selectionIndex);
                                if (quoteIndex < lastTagIndex || newText[quoteIndex - 1] != '=')
                                {
                                    InsertTextboxText("\"\"", false);
                                    textbox.TextDocument.Selection.StartPosition++;
                                }
                            }

                        }
                    }
                }
            }

            // Save the text so next time we can compare against the new text
            textbox.TextDocument.GetText(TextGetOptions.None, out m_oldText);
        }

        private void textbox_ActualThemeChanged(FrameworkElement sender, object args)
        {
            if (colorCodeToggle.IsChecked == true)
            {
                // Updating the formating for theme change
                var formatter = new XamlTextFormatter(textbox);
                formatter.ApplyColors();
            }
        }

        private void colorCodeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (colorCodeToggle.IsChecked == true)
            {
                var formatter = new XamlTextFormatter(textbox);
                formatter.ApplyColors();
            }
            else
            {
                textbox.TextDocument.GetText(TextGetOptions.UseCrlf, out string text);
                var r = textbox.TextDocument.GetRange(0, text.Length);
                r.CharacterFormat.ForegroundColor = textbox.TextDocument.GetDefaultCharacterFormat().ForegroundColor;
            }
        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (colorCodeToggle.IsChecked == true)
            {
                var formatter = new XamlTextFormatter(textbox);
                formatter.ForceTheme = ElementTheme.Light; // in UWP text boxes always use light theme resources when focused regardless of theme
                formatter.ApplyColors();
            }
        }

        private void textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (colorCodeToggle.IsChecked == true)
            {
                var formatter = new XamlTextFormatter(textbox);
                formatter.ApplyColors();
            }
        }

        private void toggleThemeButton_Checked(object sender, RoutedEventArgs e)
        {
            scratchPad.RequestedTheme = ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }

        private void toggleThemeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            scratchPad.RequestedTheme = ActualTheme == ElementTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
        }

        private void rtlToggle_Checked(object sender, RoutedEventArgs e)
        {
            scratchPad.FlowDirection = FlowDirection.RightToLeft;
        }

        private void rtlToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            scratchPad.FlowDirection = FlowDirection.LeftToRight;
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveButton.IsEnabled = false;

            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("XAML files", [".xaml"]);
            picker.SuggestedFileName = "page.xaml";
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                textbox.TextDocument.GetText(TextGetOptions.UseCrlf, out string text);

                await FileIO.WriteTextAsync(file, text);

                var closeButton = new Button
                {
                    Content = "Close"
                };

                var locationButton = new Button
                {
                    Content = "Open file location"
                };

                var flyout = new Flyout
                {
                    Content = new StackPanel
                    {
                        Spacing = 12,
                        Children =
                        {
                            new TextBlock
                            {
                                Style = (Style)Application.Current.Resources["BaseTextBlockStyle"],
                                Text = "File saved successfully."
                            },
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 6,
                                Children =
                                {

                                    closeButton,
                                    locationButton
                                }
                            }
                        }
                    }
                };

                closeButton.Click += (s, e) => flyout.Hide();
                locationButton.Click += async (s, e) =>
                {
                    // you can't do Process.Start with explorer.exe /select,path in UWP
                    // but this exists so we can use it instead
                    await Launcher.LaunchFolderAsync(await file.GetParentAsync(),
                        new FolderLauncherOptions
                        {
                            ItemsToSelect =
                            {
                                file
                            }
                        });
                };

                flyout.ShowAt(saveButton);
            }

            saveButton.IsEnabled = true;
        }

        private async void openButton_Click(object sender, RoutedEventArgs e)
        {
            openButton.IsEnabled = false;

            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".xaml");
            picker.FileTypeFilter.Add("*");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var content = await FileIO.ReadTextAsync(file);
                textbox.TextDocument.SetText(TextSetOptions.None, content);

                if (colorCodeToggle.IsChecked == true)
                {
                    var formatter = new XamlTextFormatter(textbox);
                    formatter.ApplyColors();
                }
            }

            openButton.IsEnabled = true;
        }
    }

    public struct ScratchPadSpecialControlSetupInfo
    {
        public string SetupName { get; set; }
        public Action<UIElement> SetupAction { get; set; }

        public override string ToString() => SetupName;
    }
}
