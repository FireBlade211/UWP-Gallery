using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.Controls;
using UWPGallery.DataModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;

namespace UWPGallery
{
    public static class UIHelper
    {
        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendants().OfType<T>();
        }

        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
        {
            var queue = new Queue<DependencyObject>();
            var count1 = VisualTreeHelper.GetChildrenCount(start);

            for (int i = 0; i < count1; i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);
                yield return child;
                queue.Enqueue(child);
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var count2 = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < count2; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }

        public static UIElement? FindElementByName(UIElement element, string name)
        {
            if (element.XamlRoot != null && element.XamlRoot.Content != null)
            {
                var ele = (element.XamlRoot.Content as FrameworkElement)?.FindName(name);
                if (ele != null)
                {
                    return ele as UIElement;
                }
            }
            return null;
        }

        // Confirmation of Action
        public static void AnnounceActionForAccessibility(UIElement ue, string annoucement, string activityID)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(ue);
            peer.RaiseNotificationEvent(AutomationNotificationKind.ActionCompleted,
                                        AutomationNotificationProcessing.ImportantMostRecent, annoucement, activityID);
        }
    }

    /// <summary>
    /// A class providing functionality to support generating and copying protocol activation URIs and data packages.
    /// </summary>
    public static class ProtocolActivationClipboardHelper
    {
        /// <summary>
        /// Creates a <see cref="DataPackage"/> ready to be copied from the specified <see cref="ControlInfoDataItem"/>.
        /// </summary>
        /// <param name="item">The item to create the <see cref="DataPackage"/> for.</param>
        /// <returns>The created <see cref="DataPackage"/>.</returns>
        public static DataPackage CreateDataPackage(ControlInfoDataItem item)
        {
            var uri = GetUri(item);

            string displayName = $"{Package.Current.DisplayName} - {item.Title} Samples";

            string htmlFormat = HtmlFormatHelper.CreateHtmlFormat($"<a href='{uri}'>{displayName}</a>");

            var dataPackage = new DataPackage();
            dataPackage.SetApplicationLink(uri);
            dataPackage.SetWebLink(uri);
            dataPackage.SetText(uri.ToString());
            dataPackage.SetHtmlFormat(htmlFormat);

            dataPackage.Properties.Title = displayName;

            return dataPackage;
        }

        /// <summary>
        /// Creates a <see cref="DataPackage"/> ready to be copied from the specified <see cref="GallerySample"/> inside the specified <see cref="ControlInfoDataItem"/>.
        /// </summary>
        /// <param name="item">The item that is a parent of the <see cref="GallerySample"/> specified by <paramref name="sample"/>.</param>
        /// <param name="sample">The sample to create the <see cref="DataPackage"/> for.</param>
        /// <returns>The created <see cref="DataPackage"/>.</returns>
        public static DataPackage CreateDataPackage(ControlInfoDataItem item, GallerySample sample)
        {
            var uri = GetUri(item, sample);

            string displayName = $"{Package.Current.DisplayName} - {sample.HeaderTitle} Sample ({item.Title})";

            string htmlFormat = HtmlFormatHelper.CreateHtmlFormat($"<a href='{uri}'>{displayName}</a>");

            var dataPackage = new DataPackage();
            dataPackage.SetApplicationLink(uri);
            dataPackage.SetWebLink(uri);
            dataPackage.SetText(uri.ToString());
            dataPackage.SetHtmlFormat(htmlFormat);

            dataPackage.Properties.Title = displayName;

            return dataPackage;
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> for the requested <see cref="ControlInfoDataItem"/>.
        /// </summary>
        /// <param name="item">The item to get the URI for.</param>
        /// <returns>The URI (Uniform Resource Identifier) of the item.</returns>
        public static Uri GetUri(ControlInfoDataItem item) => new($"uwp-gallery:///page/{item.UrlName}", UriKind.Absolute);

        /// <summary>
        /// Gets the <see cref="Uri"/> for the requested <see cref="GallerySample"/> inside the specified <see cref="ControlInfoDataItem"/>.
        /// </summary>
        /// <param name="item">The item that is the parent of the <see cref="GallerySample"/> specified in <paramref name="sample"/>.</param>
        /// <param name="sample">The sample to get the URI for.</param>
        /// <returns>The URI (Uniform Resource Identifier) of the item.</returns>
        public static Uri GetUri(ControlInfoDataItem item, GallerySample sample) => new($"uwp-gallery:///page/{item.UrlName}#{sample.SampleUrl}", UriKind.Absolute);
    }

    /// <summary>
    /// Provides helper methods for storage files and directories.
    /// </summary>
    public static class StorageHelper
    {
        /// <summary>
        /// Checks whether a given file exists.
        /// </summary>
        /// <remarks>This does not work on directories as it only checks files.</remarks>
        /// <param name="fileName">The file name to check.</param>
        /// <returns><see langword="true"/> if the file exists; otherwise, <see langword="false"/>.</returns>
        public static async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return file != null;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Provides methods to track the focus state of controls that do not work with the <see cref="Control.FocusState"/> property.
    /// </summary>
    public static class FocusHelper
    {
        private static Dictionary<Control, bool> FocusStates = [];

        /// <summary>
        /// Starts tracking the focus state of the specified control.
        /// </summary>
        /// <param name="c">The control to track.</param>
        public static void StartTrackFocusState(Control c)
        {
            if (!FocusStates.ContainsKey(c))
            {
                FocusStates[c] = false;

                c.GotFocus += (s, e) => FocusStates[c] = true;
                c.LostFocus += (s, e) => FocusStates[c] = false;
            }
        }

        /// <summary>
        /// Retrieves whether the control is focused.
        /// </summary>
        /// <param name="c">The control to retrieve the focus state for.</param>
        /// <returns><see langword="true"/> if the control is tracked and focused; otherwise, <see langword="false"/>.</returns>
        public static bool IsFocused(Control c) => FocusStates.ContainsKey(c) ? FocusStates[c] : false;
    }
}
