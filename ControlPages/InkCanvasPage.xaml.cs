using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InkCanvasPage : Page
    {
        private InkAnalyzer inkAnalyzer = new InkAnalyzer();
        private IReadOnlyList<InkStroke>? inkStrokes = null;
        private InkAnalysisResult? inkAnalysisResults = null;
        private InkRecognizerContainer? inkRecognizerContainer;

        public InkCanvasPage()
        {
            InitializeComponent();

            InkCanvasSample1.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            InkCanvasRecognitionSample.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            InternationalRecognitionInkCanvasSample.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            InkToolbarCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;

            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            InternationalRecognitionInkCanvasSample.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
        }

        private async void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            inkStrokes = InkCanvasRecognitionSample.InkPresenter.StrokeContainer.GetStrokes();
            // Ensure an ink stroke is present.
            if (inkStrokes.Count > 0)
            {
                inkAnalyzer.AddDataForStrokes(inkStrokes);

                // In this example, we try to recognizing both 
                // writing and drawing, so the platform default 
                // of "InkAnalysisStrokeKind.Auto" is used.
                // If you're only interested in a specific type of recognition,
                // such as writing or drawing, you can constrain recognition 
                // using the SetStrokDataKind method as follows:
                // foreach (var stroke in strokesText)
                // {
                //     analyzerText.SetStrokeDataKind(
                //      stroke.Id, InkAnalysisStrokeKind.Writing);
                // }
                // This can improve both efficiency and recognition results.
                inkAnalysisResults = await inkAnalyzer.AnalyzeAsync().AsTask();

                // Have ink strokes on the canvas changed?
                if (inkAnalysisResults.Status == InkAnalysisStatus.Updated)
                {
                    // Find all strokes that are recognized as handwriting and 
                    // create a corresponding ink analysis InkWord node.
                    var inkwordNodes =
                        inkAnalyzer.AnalysisRoot.FindNodes(
                            InkAnalysisNodeKind.InkWord);

                    // Iterate through each InkWord node.
                    // Draw primary recognized text on recognitionCanvas 
                    // (for this example, we ignore alternatives), and delete 
                    // ink analysis data and recognized strokes.
                    foreach (InkAnalysisInkWord node in inkwordNodes)
                    {
                        // Draw a TextBlock object on the recognitionCanvas.
                        DrawText(node.RecognizedText, node.BoundingRect, RecognitionCanvas);

                        foreach (var strokeId in node.GetStrokeIds())
                        {
                            var stroke =
                                InkCanvasRecognitionSample.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                            stroke.Selected = true;
                        }
                        inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
                    }
                    InkCanvasRecognitionSample.InkPresenter.StrokeContainer.DeleteSelected();

                    // Find all strokes that are recognized as a drawing and 
                    // create a corresponding ink analysis InkDrawing node.
                    var inkdrawingNodes =
                        inkAnalyzer.AnalysisRoot.FindNodes(
                            InkAnalysisNodeKind.InkDrawing);
                    // Iterate through each InkDrawing node.
                    // Draw recognized shapes on recognitionCanvas and
                    // delete ink analysis data and recognized strokes.
                    foreach (InkAnalysisInkDrawing node in inkdrawingNodes)
                    {
                        if (node.DrawingKind == InkAnalysisDrawingKind.Drawing)
                        {
                            // Catch and process unsupported shapes (lines and so on) here.
                        }
                        // Process generalized shapes here (ellipses and polygons).
                        else
                        {
                            // Draw an Ellipse object on the recognition Canvas (circle is a specialized ellipse).
                            if (node.DrawingKind == InkAnalysisDrawingKind.Circle || node.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                            {
                                DrawEllipse(node);
                            }
                            // Draw a Polygon object on the recognition Canvas.
                            else
                            {
                                DrawPolygon(node);
                            }
                            foreach (var strokeId in node.GetStrokeIds())
                            {
                                var stroke = InkCanvasRecognitionSample.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                                stroke.Selected = true;
                            }
                        }
                        inkAnalyzer.RemoveDataForStrokes(node.GetStrokeIds());
                    }
                    InkCanvasRecognitionSample.InkPresenter.StrokeContainer.DeleteSelected();

                    if (!inkwordNodes.Any() && !inkdrawingNodes.Any())
                    {
                        _ = new ContentDialog
                        {
                            Title = "Ink analysis nodes not found",
                            Content = "The ink analysis nodes were not found, meaning none of your strokes were recognized as text or drawings. Please try again.",
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close
                        }.ShowAsync();
                    }
                }
                else
                {
                    _ = new ContentDialog
                    {
                        Title = "The ink analysis status was not updated",
                        Content = "The ink analysis status was not updated, which means that some error may've occured." +
                        $" The current status is: {inkAnalysisResults.Status}",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    }.ShowAsync();
                }
            }
        }

        private void DrawText(string recognizedText, Rect boundingRect, Canvas canvas)
        {
            TextBlock text = new TextBlock();
            Canvas.SetTop(text, boundingRect.Top);
            Canvas.SetLeft(text, boundingRect.Left);

            text.Text = recognizedText;
            text.FontSize = boundingRect.Height;
            text.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

            canvas.Children.Add(text);
        }

        private void DrawEllipse(InkAnalysisInkDrawing shape)
        {
            var points = shape.Points;
            Ellipse ellipse = new Ellipse();

            ellipse.Width = shape.BoundingRect.Width;
            ellipse.Height = shape.BoundingRect.Height;

            Canvas.SetTop(ellipse, shape.BoundingRect.Top);
            Canvas.SetLeft(ellipse, shape.BoundingRect.Left);

            var brush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 255));
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 2;
            RecognitionCanvas.Children.Add(ellipse);
        }

        // Draw a polygon on the recognitionCanvas.
        private void DrawPolygon(InkAnalysisInkDrawing shape)
        {
            List<Point> points = new List<Point>(shape.Points);
            Polygon polygon = new Polygon();

            foreach (Point point in points)
            {
                polygon.Points.Add(point);
            }

            var brush = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 255));
            polygon.Stroke = brush;
            polygon.StrokeThickness = 2;
            RecognitionCanvas.Children.Add(polygon);
        }

        private void RefreshRecognizersLink_Click(object sender, RoutedEventArgs e)
        {
            inkRecognizerContainer = new InkRecognizerContainer();
            // Retrieve the collection of installed handwriting recognizers.
            IReadOnlyList<InkRecognizer> installedRecognizers =
                inkRecognizerContainer.GetRecognizers();
            // inkRecognizerContainer is null if a recognition engine is not available.
            if (!(inkRecognizerContainer == null))
            {
                RecognizerComboBox.ItemsSource = installedRecognizers.ToList();
            }
        }

        private async void InternationalRecognitionRecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes =
                InternationalRecognitionInkCanvasSample.InkPresenter.StrokeContainer.GetStrokes();

            if (!currentStrokes.Any())
            {
                _ = new ContentDialog
                {
                    Title = "No ink strokes",
                    Content = "Please draw ink strokes on the canvas before trying to recognize.",
                    CloseButtonText = "Close",
                    DefaultButton = ContentDialogButton.Close
                }.ShowAsync();

                return;
            }

            try
            {
                // Recognize all ink strokes on the ink canvas.
                IReadOnlyList<InkRecognitionResult> recognitionResults =
                    await inkRecognizerContainer?.RecognizeAsync(
                        InternationalRecognitionInkCanvasSample.InkPresenter.StrokeContainer,
                        InkRecognitionTarget.All);

                string str = string.Empty;
                int c = 0;
                // Iterate through the recognition results.
                foreach (InkRecognitionResult result in recognitionResults)
                {
                    // Get all recognition candidates from each recognition result.
                    IReadOnlyList<string> candidates =
                        result.GetTextCandidates();
                    c += candidates.Count;
                    foreach (string candidate in candidates)
                    {
                        str += candidate + "\n";
                    }

                    if (!InternationalRecognitionModeToggleSwitch.IsOn)
                        if (candidates.Any())
                            DrawText(candidates[0], result.BoundingRect, InternationalRecognitionOverlayCanvas);
                }
                // Display the recognition candidates.

                if (InternationalRecognitionModeToggleSwitch.IsOn)
                {
                    var dlg = new ContentDialog
                    {
                        Content = str,
                        Title = "Ink Recognition Results: " + c,
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close
                    };
                    _ = dlg.ShowAsync();
                }

                // Clear the ink canvas once recognition is complete.
                InternationalRecognitionInkCanvasSample.InkPresenter.StrokeContainer.Clear();
            }
            catch (Exception ex)
            {
                var btn = await new ContentDialog
                {
                    Title = "An error occured",
                    Content = "An exception occured recognizing the ink strokes.\n" +
                    $"Type: {ex.GetType().FullName ?? ex.GetType().Name}\n" +
                    $"HRESULT: {ex.HResult}\n" +
                    $"Message: \"{ex.Message}\"\n\n" +
                    "Would you like to clear the ink canvas?",
                    PrimaryButtonText = "Clear canvas",
                    SecondaryButtonText = "No",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();

                if (btn == ContentDialogResult.Primary)
                {
                    InternationalRecognitionInkCanvasSample.InkPresenter.StrokeContainer.Clear();
                }
            }
        }

        private void RecognizerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inkRecognizerContainer?.SetDefaultRecognizer(RecognizerComboBox.SelectedItem as InkRecognizer);
            InternationalRecognitionRecognizeButton.IsEnabled = true;
        }

        private async void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("uwp-gallery:///page/inktoolbar"));
        }
    }
}
