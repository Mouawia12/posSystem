using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Helpers;

namespace Infrastructure.Data
{
    public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
    {
        public PosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();
            optionsBuilder.UseSqlite($"Data Source={AppPaths.GetDatabasePath()}");
            return new PosDbContext(optionsBuilder.Options);
        }
    }
}
