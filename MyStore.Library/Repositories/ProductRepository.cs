using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.DataAccess;

namespace MyStore.Library.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly MyStoreContext _context;

        public ProductRepository(MyStoreContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync() =>
            await _context.Products.AsNoTracking().ToListAsync();

        public async Task<List<Product>> GetAllWithCategoryAsync() =>
            await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.ProductId)
                .ToListAsync();

        public async Task<Product?> GetByIdAsync(object id)
        {
            if (id is int intId)
            {
                return await _context.Products.FindAsync(intId);
            }
            return null;
        }

        public async Task AddAsync(Product entity)
        {
            await _context.Products.AddAsync(entity);
        }

        public void Update(Product entity)
        {
            _context.Products.Update(entity);
        }

        public void Delete(Product entity)
        {
            _context.Products.Remove(entity);
        }

        public Task<int> SaveAsync() => _context.SaveChangesAsync();
    }
}
