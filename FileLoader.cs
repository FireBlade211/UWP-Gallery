using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPGallery
{
    internal class FileLoader
    {
        public static async Task<string> LoadText(string relativeFilePath)
        {
            Uri sourceUri = new("ms-appx:///" + relativeFilePath);
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(sourceUri);

            return await FileIO.ReadTextAsync(file);
        }

        public static async Task<IList<string>> LoadLines(string relativeFilePath)
        {
            string fileContents = await LoadText(relativeFilePath);
            return [.. fileContents.Split(Environment.NewLine)];
        }
    }
}
