using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Compta.Ledger.Core.orgTestapp.Extention;

namespace LocalizarionApp.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult Set(string culture, string redirectUri)
    {
        if (culture != null)
        {
            var cookieValue = $"c={culture}|uic={culture}";

            HttpContext.Response.Cookies.Append(
                CookieCultureProvider.CookieName, 
                cookieValue,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMonths(1),
                    IsEssential = true,
                    Path = "/",
                    SameSite = SameSiteMode.Lax
                });
        }

        return LocalRedirect(redirectUri);
    }
}
