using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.DataAccess;

namespace MyStore.Library.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MyStoreContext _context;

        public CategoryRepository(MyStoreContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync() =>
            await _context.Categories.AsNoTracking().ToListAsync();

        public async Task<List<Category>> GetAllOrderedAsync() =>
            await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();

        public async Task<Category?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await _context.Categories.FindAsync(intId);
            }
            return null;
        }

        public async Task AddAsync(Category entity)
        {
            await _context.Categories.AddAsync(entity);
        }

        public void Update(Category entity)
        {
            _context.Categories.Update(entity);
        }

        public void Delete(Category entity)
        {
            _context.Categories.Remove(entity);
        }

        public Task<int> SaveAsync() => _context.SaveChangesAsync();
    }
}
