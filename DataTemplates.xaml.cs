using System;
using System.Linq;
using UWPGallery.DataModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPGallery
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataTemplates : ResourceDictionary
    {
        public DataTemplates()
        {
            InitializeComponent();
        }

        private void ControlItemTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControl uc)
            {
                if (uc.ContextFlyout is MenuFlyout mf)
                {
                    if (uc.DataContext is ControlInfoDataItem cidi)
                    {
                        foreach (var item in mf.Items.OfType<MenuFlyoutItem>())
                        {
                            item.Click += item.Tag switch
                            {
                                "OpenNewWindowMenuFlyoutItem" => async (s, e) =>
                                {
                                    await Launcher.LaunchUriAsync(ProtocolActivationClipboardHelper.GetUri(cidi)).AsTask();
                                },
                                "CopyLinkMenuFlyoutItem" => (s, e) =>
                                {
                                    Clipboard.SetContent(ProtocolActivationClipboardHelper.CreateDataPackage(cidi));
                                },
                                "ShareMenuFlyoutItem" => (s, e) =>
                                {
                                    App.ShareUIData = cidi;

                                    DataTransferManager.ShowShareUI();
                                },
                                _ => (s, e) => { }
                            };
                        }
                    }
                }
            }
        }
    }
}
