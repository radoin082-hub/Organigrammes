using Compta.Ledger.Core.orgTestapp.Services;
using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add Database Context
builder.Services.AddDbContext<OrgDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		x => x.UseHierarchyId()));

builder.Services.AddControllersWithViews()
	.AddApplicationPart(typeof(Compta.Ledger.Core.orgTestapp.Components.App).Assembly);

// Add Storage Layer
builder.Services.AddScoped<IOrgStorage, OrgStorage>();

// Add Organization Service
builder.Services.AddScoped<IOrgService, OrgService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<Compta.Ledger.Core.orgTestapp.Components.App>()
	.AddInteractiveServerRenderMode();

app.Run();
