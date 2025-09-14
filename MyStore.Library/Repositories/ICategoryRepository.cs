using System.Collections.Generic;
using System.Threading.Tasks;
using MyStore.Library.Data;

namespace MyStore.Library.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetAllOrderedAsync();
    }
}
