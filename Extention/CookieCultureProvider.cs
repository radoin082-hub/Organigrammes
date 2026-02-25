using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Compta.Ledger.Core.orgTestapp.Extention;

public class CookieCultureProvider : RequestCultureProvider
{
    public static readonly string CookieName = "Culture";

    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext is null)        
            throw new ArgumentNullException(nameof(httpContext));
       
        var cookie = httpContext.Request.Cookies[CookieName];

        if (string.IsNullOrEmpty(cookie))       
            return null;
        

        var culture = ParseCultureCookie(cookie);
        return culture;
    }

    private static ProviderCultureResult? ParseCultureCookie(string cookie)
    {
        if (cookie.StartsWith("c=", StringComparison.OrdinalIgnoreCase))
        {
            var parts = cookie.Split('|');
            var culture = parts[0].Substring(2);
            var uiCulture = parts.Length > 1 ? parts[1].Substring(4) : culture;
            
            return new ProviderCultureResult(culture, uiCulture);
        }

        return null;
    }
}
