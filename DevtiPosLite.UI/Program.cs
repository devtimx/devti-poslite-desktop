using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Data;
using DevtiPosLite.Data.Repositories;
using DevtiPosLite.Services;
using DevtiPosLite.UI.Forms;
using DevtiPosLite.UI.Helpers;
using DevtiPosLite.UI.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevtiPosLite.UI;

static class Program
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        ImageHelper.Initialize(basePath);
        var dbFilePath = Path.Combine(basePath, "poslite.db");

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbFilePath}"));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<ICashierService, CashierService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IConfigService, ConfigService>();

        services.AddSingleton<AuthStore>();

        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<PosForm>();
        services.AddTransient<ProductsForm>();
        services.AddTransient<CategoriesForm>();
        services.AddTransient<DenominationsForm>();
        services.AddTransient<ReportsForm>();
        services.AddTransient<CashoutForm>();
        services.AddTransient<CashHistoryForm>();
        services.AddTransient<UsersForm>();
        services.AddTransient<RolesForm>();
        services.AddTransient<PermissionsForm>();
        services.AddTransient<ConfigForm>();
        services.AddTransient<AboutForm>();

        ServiceProvider = services.BuildServiceProvider();

        try
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!db.Database.CanConnect())
                db.CreateSchema();
            else
                db.MigrateSchema();
            db.SeedAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var msg = $"Error BD:\n{ex.GetType().Name}: {ex.Message}";
            while (ex.InnerException != null) { ex = ex.InnerException; msg += $"\n→ {ex.Message}"; }
            MessageBox.Show(msg, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var authStore = ServiceProvider.GetRequiredService<AuthStore>();

        while (true)
        {
            using var loginScope = ServiceProvider.CreateScope();
            var loginForm = loginScope.ServiceProvider.GetRequiredService<LoginForm>();
            if (loginForm.ShowDialog() != DialogResult.OK)
                return;

            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);

            if (!authStore.IsAuthenticated)
                continue;
            break;
        }
    }
}
