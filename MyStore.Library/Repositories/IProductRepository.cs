using System.Collections.Generic;
using System.Threading.Tasks;
using MyStore.Library.Data;

namespace MyStore.Library.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetAllWithCategoryAsync();
    }
}
