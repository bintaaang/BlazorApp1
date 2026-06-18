using BlazorApp1.Models;

namespace BlazorApp1.Services.Saas;

public interface ISaasDashboardService
{
    Task<SaasDashboardSummary> GetSummaryAsync();
}
