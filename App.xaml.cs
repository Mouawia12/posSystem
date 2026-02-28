using Application.Services;
using Application.BusinessRules;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Resources.Localization;
using Presentation.ViewModels;
using Presentation.Views;
using Shared.Constants;
using Shared.Helpers;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace posSystem
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContextFactory<PosDbContext>(options =>
                        options.UseSqlite($"Data Source={AppPaths.GetDatabasePath()}"));

                    services.AddScoped(typeof(Core.Interfaces.IRepository<>), typeof(EfRepository<>));
                    services.AddScoped<Core.Interfaces.IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IProductSearchService, ProductSearchService>();
                    services.AddScoped<IProductManagementService, ProductManagementService>();
                    services.AddScoped<ICustomerManagementService, CustomerManagementService>();
                    services.AddScoped<IWarrantyManagementService, WarrantyManagementService>();
                    services.AddScoped<IMaintenanceManagementService, MaintenanceManagementService>();
                    services.AddScoped<IReportingService, ReportingService>();
                    services.AddScoped<IDashboardService, DashboardService>();
                    services.AddScoped<ISettingsService, SettingsService>();
                    services.AddScoped<IUserManagementService, UserManagementService>();
                    services.AddScoped<IBackupRestoreService, BackupRestoreService>();
                    services.AddScoped<IPrintingService, PrintingService>();
                    services.AddScoped<IInvoiceService, InvoiceService>();
                    services.AddSingleton<IAuthenticationService, AuthenticationService>();
                    services.AddSingleton<IStartupPrerequisitesService, StartupPrerequisitesService>();
                    services.AddSingleton<IDemoDataSeederService, DemoDataSeederService>();
                    services.AddScoped<IWarrantyPolicyService, WarrantyPolicyService>();
                    services.AddScoped<IMaintenanceScheduleGenerator, MaintenanceScheduleGenerator>();
                    services.AddSingleton<IUserContextService, UserContextService>();
                    services.AddSingleton<IPermissionService, PermissionService>();
                    services.AddSingleton<ILocalizationService, LocalizationService>();

                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            DispatcherUnhandledException += (_, args) =>
            {
                File.AppendAllText(AppPaths.GetLogPath(), $"{DateTime.UtcNow:u} UI ERROR: {args.Exception}{Environment.NewLine}");
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                File.AppendAllText(AppPaths.GetLogPath(), $"{DateTime.UtcNow:u} DOMAIN ERROR: {args.ExceptionObject}{Environment.NewLine}");
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var startupStopwatch = Stopwatch.StartNew();
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
                await using var db = await dbContextFactory.CreateDbContextAsync();
                await db.Database.MigrateAsync();
            }

            var prerequisitesService = _host.Services.GetRequiredService<IStartupPrerequisitesService>();
            var prerequisiteResult = await prerequisitesService.ValidateAndPrepareAsync();
            if (!prerequisiteResult.Passed)
            {
                throw new InvalidOperationException(string.Join(" | ", prerequisiteResult.Messages));
            }

            var authService = _host.Services.GetRequiredService<IAuthenticationService>();
            var userContext = _host.Services.GetRequiredService<IUserContextService>();
            var demoSeeder = _host.Services.GetRequiredService<IDemoDataSeederService>();
            var bootstrapPassword = Environment.GetEnvironmentVariable("POS_DEFAULT_OWNER_PASSWORD") ?? "owner12345";
            var shouldSeedDemo = e.Args.Contains("--seed-demo-data", StringComparer.OrdinalIgnoreCase)
                || string.Equals(Environment.GetEnvironmentVariable("POS_SEED_DEMO_DATA"), "1", StringComparison.OrdinalIgnoreCase);
            var forceSeedDemo = e.Args.Contains("--seed-demo-force", StringComparer.OrdinalIgnoreCase)
                || string.Equals(Environment.GetEnvironmentVariable("POS_SEED_DEMO_FORCE"), "1", StringComparison.OrdinalIgnoreCase);
            var seedOnly = e.Args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase);

            await authService.EnsureDefaultOwnerAsync(bootstrapPassword);
            var login = await authService.AuthenticateAsync("owner", bootstrapPassword);
            if (!login.Succeeded)
            {
                throw new InvalidOperationException($"Startup authentication failed: {login.Message}");
            }

            userContext.SetUser(login.UserId, login.Username, login.Role);
            await demoSeeder.EnsureSeededAsync(forceSeedDemo || shouldSeedDemo);

            if (seedOnly)
            {
                Shutdown();
                return;
            }

            _host.Services.GetRequiredService<MainWindow>().Show();
            startupStopwatch.Stop();
            if (startupStopwatch.Elapsed > TimeSpan.FromSeconds(PerformanceTargets.AppStartupSeconds))
            {
                File.AppendAllText(
                    AppPaths.GetLogPath(),
                    $"{DateTime.UtcNow:u} PERF WARNING: Startup took {startupStopwatch.Elapsed.TotalMilliseconds:F0} ms (target <= {PerformanceTargets.AppStartupSeconds * 1000} ms).{Environment.NewLine}");
            }
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
