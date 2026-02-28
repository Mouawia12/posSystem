using Application.Services;
using Application.BusinessRules;
using Core.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Resources.Localization;
using Presentation.ViewModels;
using Presentation.Views;
using Shared.Helpers;
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
                    services.AddScoped<ISettingsService, SettingsService>();
                    services.AddScoped<IBackupRestoreService, BackupRestoreService>();
                    services.AddScoped<IPrintingService, PrintingService>();
                    services.AddScoped<IInvoiceService, InvoiceService>();
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
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
                await using var db = await dbContextFactory.CreateDbContextAsync();
                await db.Database.MigrateAsync();
            }

            var userContext = _host.Services.GetRequiredService<IUserContextService>();
            userContext.SetUser(1, "owner", UserRole.Owner);

            _host.Services.GetRequiredService<MainWindow>().Show();
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
