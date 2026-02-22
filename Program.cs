using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Helpers;
using Compta.Ledger.Core.orgTestapp.Services;
using Compta.Ledger.Core.orgTestapp.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.RegisterDbcontext("DefaultConnection");
builder.Services.AddLocalization();


builder.Services.RegisterCore();

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(Compta.Ledger.Core.orgTestapp.Components.App).Assembly);
builder.Services.AddLocalization();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

// ...
string[] supportedCultures = ["en-US", "fr-FR", "ar"];
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[1])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Compta.Ledger.Core.orgTestapp.Components.App>()
	.AddInteractiveServerRenderMode();

app.Run();
