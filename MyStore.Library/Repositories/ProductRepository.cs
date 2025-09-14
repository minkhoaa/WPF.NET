using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.Data;

namespace MyStore.Library.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<MyStoreContext> _factory;

        public ProductRepository(IDbContextFactory<MyStoreContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Products.AsNoTracking().ToListAsync();
        }

        public async Task<List<Product>> GetAllWithCategoryAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.ProductId)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                await using var db = await _factory.CreateDbContextAsync();
                return await db.Products.FindAsync(intId);
            }
            return null;
        }

        public async Task AddAsync(Product entity)
        {
            await using var db = await _factory.CreateDbContextAsync();
            await db.Products.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public void Update(Product entity)
        {
            using var db = _factory.CreateDbContext();
            db.Products.Update(entity);
            db.SaveChanges();
        }

        public void Delete(Product entity)
        {
            using var db = _factory.CreateDbContext();
            db.Products.Remove(entity);
            db.SaveChanges();
        }

        public Task<int> SaveAsync() => Task.FromResult(0);
    }
}
