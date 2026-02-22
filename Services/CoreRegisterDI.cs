using Compta.Ledger.Core.orgTestapp.App;
using Compta.Ledger.Core.orgTestapp.Data;
using Compta.Ledger.Core.orgTestapp.Helpers;
using Compta.Ledger.Core.orgTestapp.Storage;
using Microsoft.EntityFrameworkCore;

namespace Compta.Ledger.Core.orgTestapp.Services;

public static class CoreRegisterDI
{

    public static void RegisterCore(this IServiceCollection services)
    {
        services.AddScoped<IOrgService, OrgService>();
        services.AddScoped<IOrgStorage, OrgStorage>();
        services.AddSingleton<IGuidProvider, GuidProvider>();
        services.AddSingleton<IDateProvider, DateProvider>();

    }
    public static void RegisterDbcontext(this WebApplicationBuilder webApp, string connectionString)
    {
        webApp.Services.AddDbContextFactory<OrgDbContext>(options =>
               options.UseSqlServer(
               webApp.Configuration.GetConnectionString(connectionString),
               x => x.UseHierarchyId()), ServiceLifetime.Singleton);

    }
}