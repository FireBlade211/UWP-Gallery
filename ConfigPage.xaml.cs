using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigPage : Page
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        private async void ConfigFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(ConfigurationStorageManager.ConfigFilePath).AsTask());
            }
            catch (FileNotFoundException)
            {
                await new ContentDialog
                {
                    Title = "Can't open the configuration file",
                    Content = "We can't open the configuration file because it does not exist yet. Please try updating some settings or marking some samples as favorite and try again.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
                }
                .ShowAsync();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "An error occured opening the configuration file",
                    Content = $"An error has occured while opening the configuration file. Please try again later.\n\n\"{ex.Message}\"",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
                }
                .ShowAsync();
            }
        }

        private async void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            if (await new ContentDialog
            {
                Title = "Are you sure you want to restore settings to defaults?",
                Content = "All your settings and other data will be lost. This includes settings and favorited samples. Are you sure?",
                PrimaryButtonText = "Restore",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
            }
            .ShowAsync() != ContentDialogResult.Primary) return;

            try
            {
                await (await StorageFile.GetFileFromPathAsync(ConfigurationStorageManager.ConfigFilePath)).DeleteAsync(StorageDeleteOption.PermanentDelete);

                await new ContentDialog
                {
                    Title = "Settings restored!",
                    Content = "The settings have been restored to defaults.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
                }
                .ShowAsync();

                if (MainPage.Current != null)
                {
                    MainPage.Current.NavBar.SelectedItem = null;
                    MainPage.Current.MainFrame.Navigate(typeof(Page));
                }
            }
            catch (FileNotFoundException)
            {
                await new ContentDialog
                {
                    Title = "Can't delete the configuration file",
                    Content = "We can't delete the settings file to clear your settings as no settings have been changed, so the file doesn't exist yet.",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
                }
                .ShowAsync();
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "An error occured deleting the configuration file",
                    Content = $"An error has occured while deleting the configuration file. Please try again later.\n\n\"{ex.Message}\"",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    RequestedTheme = MainPage.Current?.RequestedTheme ?? ElementTheme.Default
                }
                .ShowAsync();
            }
        }

        private void StartPageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void StartPageCombo_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox StartPageCombo)
            {
                var groups = await ControlInfoDataSource.Instance.GetGroupsAsync();
                var src = new List<ControlInfoDataItem>();

                foreach (var group in groups)
                {
                    foreach (var item in group.Items)
                    {
                        src.Add(item);
                    }
                }

                StartPageCombo.ItemsSource = src;
            }
        }

        private async void SystemThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                await ConfigurationStorageManager.SetAppTheme(rb.Tag switch
                {
                    "SystemThemeRadio" => ElementTheme.Default,
                    "LightThemeRadio" => ElementTheme.Light,
                    "DarkThemeRadio" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                });

                if (MainPage.Current != null)
                    await MainPage.Current.LoadConfig();
            }
        }

        private async void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel sp)
            {
                var theme = await ConfigurationStorageManager.GetAppTheme();

                switch (theme)
                {
                    case ElementTheme.Default:
                        ((RadioButton)sp.FindName("SystemThemeRadio")!).IsChecked = true;
                        break;
                    case ElementTheme.Light:
                        ((RadioButton)sp.FindName("LightThemeRadio")!).IsChecked = true;
                        break;
                    case ElementTheme.Dark:
                        ((RadioButton)sp.FindName("DarkThemeRadio")!).IsChecked = true;
                        break;
                }
            }
        }

        private async void StackPanel_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel sp)
            {
                var soundProps = await ConfigurationStorageManager.GetSoundProperties();

                if (sp.FindName("SoundSwitch") is ToggleSwitch soundSwitch)
                {
                    soundSwitch.IsOn = soundProps.IsSoundEnabled;
                }

                if (sp.FindName("VolumeSlider") is Slider volumeSlider)
                    volumeSlider.Value = soundProps.Volume * 100;

                if (sp.FindName("SpatialAudioBox") is CheckBox spatialBox)
                    spatialBox.IsChecked = soundProps.UseSpatialAudio;
            }
        }

        private async void SoundSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch SoundSwitch)
            {
                var soundProps = await ConfigurationStorageManager.GetSoundProperties();
                soundProps.IsSoundEnabled = SoundSwitch.IsOn;

                await ConfigurationStorageManager.SetSoundProperties(soundProps);

                if (MainPage.Current != null)
                    await MainPage.Current.LoadConfig();
            }
        }

        private async void VolumeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Slider VolumeSlider)
            {
                var soundProps = await ConfigurationStorageManager.GetSoundProperties();
                soundProps.Volume = VolumeSlider.Value / 100;

                await ConfigurationStorageManager.SetSoundProperties(soundProps);

                if (MainPage.Current != null)
                    await MainPage.Current.LoadConfig();
            }
        }

        private async void SpatialAudioBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox SpatialAudioBox)
            {
                var soundProps = await ConfigurationStorageManager.GetSoundProperties();
                soundProps.UseSpatialAudio = SpatialAudioBox.IsChecked ?? false;

                await ConfigurationStorageManager.SetSoundProperties(soundProps);

                _ = MainPage.Current?.LoadConfig();
            }
        }
    }
}
