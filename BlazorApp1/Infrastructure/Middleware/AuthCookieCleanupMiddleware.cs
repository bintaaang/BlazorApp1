namespace BlazorApp1.Infrastructure.Middleware;

public static class AuthCookieCleanupMiddleware
{
    public static IApplicationBuilder UseAuthCookieCleanup(
        this IApplicationBuilder app,
        string authCookiePrefix,
        string currentAuthCookieName)
    {
        return app.Use(async (httpContext, next) =>
        {
            var staleCookieNames = httpContext.Request.Cookies.Keys
                .Where(cookieName =>
                    cookieName.StartsWith(authCookiePrefix, StringComparison.Ordinal) &&
                    !string.Equals(cookieName, currentAuthCookieName, StringComparison.Ordinal))
                .ToList();

            foreach (var staleCookieName in staleCookieNames)
            {
                httpContext.Response.Cookies.Delete(staleCookieName, new CookieOptions
                {
                    Path = "/"
                });
            }

            await next();
        });
    }
}
