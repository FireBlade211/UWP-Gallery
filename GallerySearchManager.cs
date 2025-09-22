using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UWPGallery.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UWPGallery
{
    /// <summary>
    /// Provides methods related to searching and performing search-related operations for the UWP Gallery.
    /// </summary>
    public static class GallerySearchManager
    {
        /// <summary>
        /// Searches the gallery by a specified query and returns the <see cref="SearchResultContainer"/> that represents the result.
        /// </summary>
        /// <param name="query">The search query to search for.</param>
        /// <returns>The <see cref="SearchResultContainer"/> that represents the result.</returns>
        public static async Task<SearchResultContainer> SearchGallery(string query)
        {
            var container = new SearchResultContainer();
            var querySplit = query.Split(" ");

            foreach (var group in await ControlInfoDataSource.Instance.GetGroupsAsync()) // getgroupsasync automatically pulls existing copy if one is present instead of refetching from the json file
            {
                SearchResultCategory? category = container.Categories.FirstOrDefault(x => x.Name.Equals(group.Title, StringComparison.CurrentCultureIgnoreCase));

                if (category == null)
                {
                    category = new SearchResultCategory
                    {
                        Group = group
                    };

                    container.Categories.Add(category);
                }

                foreach (var item in group.Items.Where(item =>
                {
                    if (!item.IncludedInBuild) return false;

                    // Idea: check for every word entered (separated by space) if it is in the name,
                    // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                    // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                    bool flag = true;
                    foreach (string queryToken in querySplit)
                    {
                        // Check if token is in title or subtitle
                        if (!item.Title.Contains(queryToken, StringComparison.CurrentCultureIgnoreCase) &&
                            item.Subtitle != null && !item.Subtitle.Contains(queryToken, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Neither title nor sub title contain one of the tokens so we discard this item!
                            flag = false;
                        }
                    }
                    return flag;
                }).ToList())
                    category.Items.Add(item);
            }

            return container;
        }
    }
    /// <summary>
    /// A container that stores search results fetched by <see cref="GallerySearchManager.SearchGallery(string)"/>.
    /// </summary>
    public class SearchResultContainer
    {
        /// <summary>
        /// Gets or sets the categories in the search result container.
        /// </summary>
        public ObservableCollection<SearchResultCategory> Categories { get; set; } = [];

        /// <summary>
        /// Gets a flat view of the search result items in the search result container.
        /// </summary>
        public ObservableCollection<ControlInfoDataItem> Items => [.. Categories.SelectMany((cat) => cat.Items)];
    }

    /// <summary>
    /// Represents a category (filter) for search results.
    /// </summary>
    public class SearchResultCategory
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name => Group.Title;

        /// <summary>
        /// Gets the unique ID of the category.
        /// </summary>
        public string UniqueId => Group.UniqueId;

        /// <summary>
        /// Gets or sets the <see cref="ControlInfoDataGroup"/> this category represents.
        /// </summary>
        public required ControlInfoDataGroup Group { get; set; }

        /// <summary>
        /// Gets the amount of items in the category.
        /// </summary>
        public int Count => Items.Count;

        /// <summary>
        /// Gets or sets the items in the category.
        /// </summary>
        public ObservableCollection<ControlInfoDataItem> Items { get; set; } = [];

        /// <summary>
        /// Gets the display header of the category.
        /// </summary>
        public string DisplayHeader => $"{Name} ({Count})";

        /// <summary>
        /// Gets whether the category should be shown in the search results.
        /// </summary>
        public bool ShouldShow => Count > 0;
    }
}
