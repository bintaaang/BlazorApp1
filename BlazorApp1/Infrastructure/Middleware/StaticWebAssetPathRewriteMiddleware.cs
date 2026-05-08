namespace BlazorApp1.Infrastructure.Middleware;

public static class StaticWebAssetPathRewriteMiddleware
{
    private const string StaticWebAssetMarker = "/_content/";

    public static IApplicationBuilder UseStaticWebAssetPathRewrite(this IApplicationBuilder app)
    {
        return app.Use((httpContext, next) =>
        {
            var path = httpContext.Request.Path.Value;

            if (!string.IsNullOrEmpty(path))
            {
                var markerIndex = path.IndexOf(StaticWebAssetMarker, StringComparison.OrdinalIgnoreCase);
                if (markerIndex > 0)
                {
                    httpContext.Response.Redirect(path[markerIndex..], permanent: false);
                    return Task.CompletedTask;
                }
            }

            return next();
        });
    }
}
