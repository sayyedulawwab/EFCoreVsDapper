using EFCoreVsDapper.Entities;
using System.Linq.Expressions;

namespace EFCoreVsDapper;
public interface IProductRepository
{
    Task DeleteAllAsync();
    Task InsertAsync();
    Task<Product> GetByIdWithDapperAsync(long id);
    Task<Product> GetByIdWithEFCoreAsync(long id);
    Task<Product> GetByIdWithEFCoreAsNoTrackingAsync(long id);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithDapperAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithEFCoreAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);

    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByGroupByHavingPaginationWithDapperAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByGroupByHavingPaginationWithEFCoreAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);
    Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithOrderByGroupByHavingPaginationWithEFCoreAsNoTrackingAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null);


    Task InsertProductWithDapperAsync(string name, string description, int quantity, decimal price, DateTime createdOn);
    Task InsertProductWithEFCoreAsync(string name, string description, int quantity, decimal price, DateTime createdOn);
}