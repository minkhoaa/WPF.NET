using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.Data;

namespace MyStore.Library.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbContextFactory<MyStoreContext> _factory;

        public CategoryRepository(IDbContextFactory<MyStoreContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<List<Category>> GetAllOrderedAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                await using var db = await _factory.CreateDbContextAsync();
                return await db.Categories.FindAsync(intId);
            }
            return null;
        }

        public async Task AddAsync(Category entity)
        {
            await using var db = await _factory.CreateDbContextAsync();
            await db.Categories.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public void Update(Category entity)
        {
            using var db = _factory.CreateDbContext();
            db.Categories.Update(entity);
            db.SaveChanges();
        }

        public void Delete(Category entity)
        {
            using var db = _factory.CreateDbContext();
            db.Categories.Remove(entity);
            db.SaveChanges();
        }

        public Task<int> SaveAsync() => Task.FromResult(0);
    }
}
