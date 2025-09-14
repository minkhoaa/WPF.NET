using System.Collections.Generic;
using System.Threading.Tasks;
using MyStore.Library.DataAccess;

namespace MyStore.Library.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetAllWithCategoryAsync();
    }
}
