using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public sealed class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Warranty> Warranties => Set<Warranty>();
        public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
        public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();
        public DbSet<MaintenanceVisit> MaintenanceVisits => Set<MaintenanceVisit>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUsers(modelBuilder);
            ConfigureCustomers(modelBuilder);
            ConfigureProducts(modelBuilder);
            ConfigureInventoryMovements(modelBuilder);
            ConfigureInvoices(modelBuilder);
            ConfigureInvoiceItems(modelBuilder);
            ConfigurePayments(modelBuilder);
            ConfigureDevices(modelBuilder);
            ConfigureWarranties(modelBuilder);
            ConfigureMaintenance(modelBuilder);
            ConfigureSettings(modelBuilder);
            ConfigureAuditLogs(modelBuilder);
        }

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<User>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.Username).IsUnique();
        }

        private static void ConfigureCustomers(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Customer>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.Phone);
        }

        private static void ConfigureProducts(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Product>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SKU).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.SalePrice).HasPrecision(18, 2);
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.HasIndex(x => x.SKU);
            entity.HasIndex(x => x.Name);
        }

        private static void ConfigureInventoryMovements(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<InventoryMovement>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.Reason).HasMaxLength(250).IsRequired();
            entity.HasOne(x => x.Product).WithMany(x => x.InventoryMovements)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureInvoices(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Invoice>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.Tax).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Profit).HasPrecision(18, 2);
            entity.HasIndex(x => x.InvoiceNumber);
            entity.HasOne(x => x.Customer).WithMany(x => x.Invoices)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.User).WithMany(x => x.Invoices)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureInvoiceItems(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<InvoiceItem>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.Property(x => x.LineProfit).HasPrecision(18, 2);
            entity.HasOne(x => x.Invoice).WithMany(x => x.Items)
                .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany(x => x.InvoiceItems)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigurePayments(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Payment>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.Invoice).WithMany(x => x.Payments)
                .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureDevices(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Device>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DeviceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SerialNumber).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.SerialNumber);
            entity.HasOne(x => x.Customer).WithMany(x => x.Devices)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SoldInvoice).WithMany()
                .HasForeignKey(x => x.SoldInvoiceId).OnDelete(DeleteBehavior.SetNull);
        }

        private static void ConfigureWarranties(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Warranty>();
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Device).WithMany()
                .HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Customer).WithMany(x => x.Warranties)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Invoice).WithMany()
                .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureMaintenance(ModelBuilder modelBuilder)
        {
            var plans = modelBuilder.Entity<MaintenancePlan>();
            plans.HasKey(x => x.Id);
            plans.HasOne(x => x.Device).WithMany()
                .HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Restrict);
            plans.HasOne(x => x.Customer).WithMany(x => x.MaintenancePlans)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);

            var schedules = modelBuilder.Entity<MaintenanceSchedule>();
            schedules.HasKey(x => x.Id);
            schedules.HasIndex(x => x.DueDate);
            schedules.HasOne(x => x.Plan).WithMany(x => x.Schedules)
                .HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Cascade);

            var visits = modelBuilder.Entity<MaintenanceVisit>();
            visits.HasKey(x => x.Id);
            visits.Property(x => x.WorkType).HasMaxLength(200).IsRequired();
            visits.Property(x => x.Notes).HasMaxLength(2000);
            visits.Property(x => x.CostAmount).HasPrecision(18, 2);
            visits.HasOne(x => x.Schedule).WithMany(x => x.Visits)
                .HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureSettings(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Setting>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.Property(x => x.InvoicePrefix).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PrinterName).HasMaxLength(200);
        }

        private static void ConfigureAuditLogs(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<AuditLog>();
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(200).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
