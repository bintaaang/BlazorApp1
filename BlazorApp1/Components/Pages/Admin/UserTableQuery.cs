using BlazorApp1.Models.DTOs;
using BootstrapBlazor.Components;

namespace BlazorApp1.Components.Pages.Admin;

internal static class UserTableQuery
{
    public static QueryData<UserDto> Build(IEnumerable<UserDto> source, QueryPageOptions options)
    {
        var filters = options.ToFilter();
        IEnumerable<UserDto> items = source.Where(filters.GetFilterFunc<UserDto>());
        items = Search(items, options.SearchText);

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

        var totalItems = items.ToList();
        var pagedItems = totalItems
            .Skip((options.PageIndex - 1) * options.PageItems)
            .Take(options.PageItems)
            .ToList();

        return new QueryData<UserDto>
        {
            Items = pagedItems,
            TotalCount = totalItems.Count,
            IsSorted = isSorted,
            IsFiltered = filters.HasFilters(),
            IsSearch = !string.IsNullOrWhiteSpace(options.SearchText)
        };
    }

    private static IEnumerable<UserDto> Search(IEnumerable<UserDto> items, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return items;
        }

        var keyword = searchText.Trim();

        return items.Where(item =>
            item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.StatusText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.RolesText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.PermissionCount.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
