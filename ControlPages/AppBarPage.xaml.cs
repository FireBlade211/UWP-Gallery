using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppBarPage : Page
    {
        public AppBarPage()
        {
            InitializeComponent();

            DeleteButton.Command = new StandardUICommand(StandardUICommandKind.Delete);
            GenerateListBoxItems();
        }

        private void GenerateListBoxItems()
        {
            SampleListBox.Items.Clear();

            for (int i = 0; i < 16; i++)
            {
                SampleListBox.Items.Add(new ListBoxItem
                {
                    Content = $"Item {i + 1}"
                });
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SampleListBox.Items.Remove(SampleListBox.SelectedItem);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SampleListBox.Items.Add(new ListBoxItem
            {
                Content = $"Item {SampleListBox.Items.Count + 1}"
            });
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateListBoxItems();
        }
    }

    public partial class NullableObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException("Converting a boolean to an object is not possible.");
    }
}
