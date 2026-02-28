using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly PosDbContext _dbContext;

        public UnitOfWork(PosDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
