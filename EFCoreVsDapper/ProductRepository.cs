using Bogus;
using Dapper;
using EFCoreVsDapper.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EFCoreVsDapper;
public class ProductRepository : IProductRepository
{
    private readonly IDatabaseConnectionFactory _databaseFactory;
    public ProductRepository(IDatabaseConnectionFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public async Task InsertAsync()
    {
        var faker = new Faker();

        using var context = _databaseFactory.CreateEFCoreContext();

        // Seed products

        var products = new List<Product>();

        for (int i = 0; i < 100; i++)
        {
            var name = faker.Commerce.ProductName();
            var description = faker.Commerce.ProductAdjective();
            var quantity = faker.Random.Number(1, 100);
            var price = faker.Finance.Amount();
           

            var product = new Product()
            {
                Name = name,
                Description = description,
                Quantity = quantity,
                Price = price,
                CreatedOnUtc = DateTime.UtcNow,
            };

            products.Add(product);
        }
        context.Set<Product>().AddRange(products);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAllAsync()
    {
        using var connection = _databaseFactory.CreateDapperConnection();
        
        await connection.ExecuteAsync("TRUNCATE TABLE Products;");
    }


    public async Task InsertProductWithEFCoreAsync(string name, string description, int quantity, decimal price, DateTime createdOnUtc)
    {
        var product = new Product { Name = name, Description = description, Quantity = quantity, Price = price, CreatedOnUtc = createdOnUtc };
        using var context = _databaseFactory.CreateEFCoreContext();
        
        await context.Products.AddAsync(product);

        await context.SaveChangesAsync();
    }

    public async Task InsertProductWithDapperAsync(string name, string description, int quantity, decimal price, DateTime createdOn)
    {
        var query = "INSERT INTO Products(Name, Description, Quantity, Price, CreatedOnUtc) values(@Name, @Description, @Quantity, @Price, @CreatedOnUtc);";

        using var connection = _databaseFactory.CreateDapperConnection();
        
        await connection.ExecuteAsync(query, new { Name = name, Description = description, Quantity = quantity, Price = price, CreatedOnUtc = createdOn });
    }


    public async Task<Product> GetByIdWithEFCoreAsync(long id)
    {
        using var context = _databaseFactory.CreateEFCoreContext();
        
        var query = context.Products.Where(product => product.Id == id);
                
        var result = await query.FirstOrDefaultAsync();

        return result;
    }

    public async Task<Product> GetByIdWithDapperAsync(long id)
    {
        var query = "SELECT Name, Description, Quantity, Price, CreatedOnUtc, UpdatedOnUtc FROM Products WHERE Id = @Id;";

        using var connection = _databaseFactory.CreateDapperConnection();
        
        var result = await connection.QueryFirstOrDefaultAsync<Product>(query, new { Id = id });

        return result;
    }

    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithDapperAsync(string keyword, string orderBy, int page = 1, int pageSize = 10)
    {
        var offset = (page - 1) * pageSize;

       
        var queryBuilder = new StringBuilder();
        queryBuilder.Append("SELECT Name, Description, Quantity, Price, CreatedOnUtc, UpdatedOnUtc FROM Products ");


        if (!string.IsNullOrEmpty(keyword))
        {
            queryBuilder.Append("WHERE Name = @Keyword ");
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            if (orderBy == "created_on")
            {
                queryBuilder.Append("ORDER BY CreatedOn ");
            }
            else if (orderBy == "price_desc")
            {
                queryBuilder.Append("ORDER BY Price DESC ");
            }
            else if (orderBy == "price_asc")
            {
                queryBuilder.Append("ORDER BY Price ASC ");
            }
        }

        
        queryBuilder.Append($"OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");


        var countQuery = "SELECT COUNT(1) FROM Products " +
                         (string.IsNullOrEmpty(keyword) ? "" : "WHERE Name = @Keyword");

        using var connection = _databaseFactory.CreateDapperConnection();

        var products = await connection.QueryAsync<Product>(queryBuilder.ToString(), new { Keyword = keyword, Offset = offset, PageSize = pageSize });

        var totalRecords = await connection.ExecuteScalarAsync<int>(countQuery, new { Keyword = keyword });

        return (products.ToList(), totalRecords);
    }

    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithEFCoreAsync(string keyword, string orderBy, int page = 1, int pageSize = 10)
    {
        using var context = _databaseFactory.CreateEFCoreContext();
        
        IQueryable<Product> query = context.Set<Product>();


        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Name == keyword);
        }

        var totalCount = await query.CountAsync();

        if (!string.IsNullOrEmpty(orderBy))
        {
            if (orderBy == "created_on")
            {
                query = query.OrderBy(p => p.CreatedOnUtc);
            }
            else if (orderBy == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else if (orderBy == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
        }

        var pagedProducts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pagedProducts, totalCount);
    }

  
}
