using EFCoreVsDapper.Entities;
using System.Linq.Expressions;

namespace EFCoreVsDapper;
public interface IProductRepository
{
    Task DeleteAllAsync();
    Task InsertAsync();
    Task<Product> GetByIdWithDapperAsync(long id);
    Task<Product> GetByIdWithEFCoreAsync(long id);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithEFCoreAsync(string keyword, string orderBy,
                                                        int page = 1,
                                                        int pageSize = 10);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithDapperAsync(string keyword, string orderBy,
                                                        int page = 1,
                                                        int pageSize = 10);
    Task InsertProductWithDapperAsync(string name, string description, int quantity, decimal price, DateTime createdOn);
    Task InsertProductWithEFCoreAsync(string name, string description, int quantity, decimal price, DateTime createdOn);


}