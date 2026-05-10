using BootstrapBlazor.Components;

namespace BlazorApp1.Infrastructure.Table;

public static class TableQuery
{
    public static Task<QueryData<TItem>> BuildAsync<TItem>(
        IEnumerable<TItem> source,
        QueryPageOptions options,
        Func<TItem, string, bool>? search = null)
    {
        var filters = options.ToFilter();
        var items = source.Where(filters.GetFilterFunc<TItem>());

        if (!string.IsNullOrWhiteSpace(options.SearchText) && search != null)
        {
            var keyword = options.SearchText.Trim();
            items = items.Where(item => search(item, keyword));
        }

        var isSorted = false;
        if (options.AdvancedSortList.Count != 0)
        {
            items = items.Sort(options.AdvancedSortList);
            isSorted = true;
        }
        else if (!string.IsNullOrEmpty(options.SortName))
        {
            items = items.Sort(options.SortName, options.SortOrder);
            isSorted = true;
        }

        var allItems = items.ToList();
        var pageItems = allItems
            .Skip((options.PageIndex - 1) * options.PageItems)
            .Take(options.PageItems)
            .ToList();

        return Task.FromResult(new QueryData<TItem>
        {
            Items = pageItems,
            TotalCount = allItems.Count,
            IsSorted = isSorted,
            IsFiltered = filters.HasFilters(),
            IsSearch = !string.IsNullOrWhiteSpace(options.SearchText)
        });
    }
}
