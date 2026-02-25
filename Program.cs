using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Components;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Extention;
using Compta.Ledger.Core.orgTestapp.Helpers;
using Compta.Ledger.Core.orgTestapp.Services;
using Compta.Ledger.Core.orgTestapp.Storage;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddControllers();

builder.RegisterDbcontext("DefaultConnection");
builder.Services.AddLocalization();
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR"),
    new CultureInfo("ar-SA")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new CookieCultureProvider());
});

builder.Services.RegisterCore();

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(App).Assembly);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}
app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseHttpsRedirection();

app.MapControllers();
app.UseRequestLocalization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
