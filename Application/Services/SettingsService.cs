using Application.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public SettingsService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            if (settings is null)
            {
                settings = new Setting
                {
                    CompanyName = "My Retail Store",
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1,
                    Currency = "USD"
                };

                db.Settings.Add(settings);
                await db.SaveChangesAsync(cancellationToken);
            }

            return new SettingsDto
            {
                CompanyName = settings.CompanyName,
                Address = settings.Address,
                Phone = settings.Phone,
                InvoicePrefix = settings.InvoicePrefix,
                NextInvoiceNumber = settings.NextInvoiceNumber,
                Currency = settings.Currency,
                PrinterName = settings.PrinterName
            };
        }

        public async Task SaveAsync(SettingsDto settings, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Settings.FirstOrDefaultAsync(cancellationToken);

            if (entity is null)
            {
                entity = new Setting();
                db.Settings.Add(entity);
            }

            entity.CompanyName = settings.CompanyName.Trim();
            entity.Address = settings.Address?.Trim();
            entity.Phone = settings.Phone?.Trim();
            entity.InvoicePrefix = settings.InvoicePrefix.Trim();
            entity.NextInvoiceNumber = settings.NextInvoiceNumber;
            entity.Currency = settings.Currency.Trim();
            entity.PrinterName = settings.PrinterName?.Trim();

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
