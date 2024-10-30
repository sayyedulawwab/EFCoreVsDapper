using Bogus;
using Dapper;
using EFCoreVsDapper.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EFCoreVsDapper;
public class ProductRepository : IProductRepository
{
    private readonly IDatabaseConnectionFactory _databaseFactory;
    private readonly int _timeoutInSeconds;
    public ProductRepository(IDatabaseConnectionFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
        _timeoutInSeconds = 600;
    }   

    public async Task InsertAsync()
    {
        var faker = new Faker();

        await using var context = _databaseFactory.CreateEFCoreContext();
        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        // Seed products

        var products = new List<Product>();

        //Parallel.For(0, 1_000_000, i =>
        //{
        //    lock (products)
        //    {
        //        products.Add(new Product
        //        {
        //            Name = faker.Commerce.ProductName(),
        //            Description = faker.Commerce.ProductAdjective(),
        //            Quantity = faker.Random.Number(1, 100),
        //            Price = faker.Finance.Amount(25, 2000, 2),
        //            CreatedOnUtc = DateTime.UtcNow,
        //        });
        //    }
        //});

        for (int i = 0; i < 1_000_000; i++)
        {
            var name = faker.Commerce.ProductName();
            var description = faker.Commerce.ProductAdjective();
            var quantity = faker.Random.Number(1, 100);
            var price = faker.Finance.Amount(25, 2000, 2);


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

        int batchSize = 10_000;
        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize);
            context.Set<Product>().AddRange(batch);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
       
    }

    public async Task DeleteAllAsync()
    {
        using var connection = _databaseFactory.CreateDapperConnection();

        
        await connection.ExecuteAsync("TRUNCATE TABLE Products;");
    }


    public async Task InsertProductWithEFCoreAsync(string name, string description, int quantity, decimal price, DateTime createdOnUtc)
    {
        var product = new Product { Name = name, Description = description, Quantity = quantity, Price = price, CreatedOnUtc = createdOnUtc };

        await using var context = _databaseFactory.CreateEFCoreContext();
        
        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        await context.Products.AddAsync(product);

        await context.SaveChangesAsync();
    }

    public async Task InsertProductWithDapperAsync(string name, string description, int quantity, decimal price, DateTime createdOn)
    {
        var query = "INSERT INTO Products(Name, Description, Quantity, Price, CreatedOnUtc) values(@Name, @Description, @Quantity, @Price, @CreatedOnUtc);";

        using var connection = _databaseFactory.CreateDapperConnection();
        
        await connection.ExecuteAsync(query, 
                        new { Name = name, Description = description, Quantity = quantity, Price = price, CreatedOnUtc = createdOn }, 
                        commandTimeout: _timeoutInSeconds);
    }


    public async Task<Product> GetByIdWithEFCoreAsync(long id)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        var query = context.Products.Where(product => product.Id == id);
                
        var result = await query.FirstOrDefaultAsync();

        return result;
    }

    public async Task<Product> GetByIdWithEFCoreAsNoTrackingAsync(long id)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        var query = context.Products.Where(product => product.Id == id);

        var result = await query.AsNoTracking().FirstOrDefaultAsync();

        return result;
    }

    public async Task<Product> GetByIdWithDapperAsync(long id)
    {
        var query = "SELECT Name, Description, Quantity, Price, CreatedOnUtc, UpdatedOnUtc FROM Products WHERE Id = @Id;";

        using var connection = _databaseFactory.CreateDapperConnection();
        
        var result = await connection.QueryFirstOrDefaultAsync<Product>(query, new { Id = id }, commandTimeout: _timeoutInSeconds);

        return result;
    }


    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithDapperAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                               decimal? minPrice = null, decimal? maxPrice = null)
    {
        var offset = (page - 1) * pageSize;

        var queryBuilder = new StringBuilder();

        queryBuilder.Append(@"SELECT p1.Id, p1.Name, p1.Description, p1.Quantity, p1.Price, p1.CreatedOnUtc, p1.UpdatedOnUtc
        FROM Products p1 ");

        // Adding WHERE conditions dynamically
        queryBuilder.Append("WHERE 1=1 "); // Ensures any additional WHERE conditions can be appended safely



        if (minPrice.HasValue)
        {
            queryBuilder.Append("AND p1.Price >= @MinPrice ");
        }

        if (maxPrice.HasValue)
        {
            queryBuilder.Append("AND p1.Price <= @MaxPrice ");
        }

       
        // Adding ORDER BY clause
        if (!string.IsNullOrEmpty(orderBy))
        {
            if (orderBy == "created_on")
            {
                queryBuilder.Append("ORDER BY p1.CreatedOnUtc ");
            }
            else if (orderBy == "price_desc")
            {
                queryBuilder.Append("ORDER BY p1.Price DESC ");
            }
            else if (orderBy == "price_asc")
            {
                queryBuilder.Append("ORDER BY p1.Price ASC ");
            }
        }

        // Adding pagination
        queryBuilder.Append("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        // Count query for total records (without pagination)
        var countQuery = @"SELECT COUNT(1) FROM Products p1 WHERE 1=1 " +
            (minPrice.HasValue ? "AND p1.Price >= @MinPrice " : "") +
            (maxPrice.HasValue ? "AND p1.Price <= @MaxPrice " : "");

        using var connection = _databaseFactory.CreateDapperConnection();

        // Execute product query
        var products = await connection.QueryAsync<Product>(
            queryBuilder.ToString(),
            new { Offset = offset, PageSize = pageSize, MinPrice = minPrice, MaxPrice = maxPrice },
            commandTimeout: _timeoutInSeconds
        );

        // Execute count query
        var totalRecords = await connection.ExecuteScalarAsync<int>(
            countQuery,
            new { MinPrice = minPrice, MaxPrice = maxPrice },
            commandTimeout: _timeoutInSeconds
        );

        return (products.ToList(), totalRecords);
    }


    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithEFCoreAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                 decimal? minPrice = null, decimal? maxPrice = null)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        // Base query using self-join with LINQ
        var query = from p1 in context.Set<Product>()
                    select p1;

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Ordering results dynamically
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = orderBy switch
            {
                "created_on" => query.OrderBy(p => p.CreatedOnUtc),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "price_asc" => query.OrderBy(p => p.Price),
                _ => query
            };
        }

        // Fetching total record count before pagination
        var totalCount = await query.CountAsync();

        // Applying pagination
        var pagedProducts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pagedProducts, totalCount);
    }

    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                 decimal? minPrice = null, decimal? maxPrice = null)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        // Base query using self-join with LINQ
        var query = from p1 in context.Set<Product>().AsNoTracking()
                    select p1;

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Ordering results dynamically
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = orderBy switch
            {
                "created_on" => query.OrderBy(p => p.CreatedOnUtc),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "price_asc" => query.OrderBy(p => p.Price),
                _ => query
            };
        }

        // Fetching total record count before pagination
        var totalCount = await query.CountAsync();

        // Applying pagination
        var pagedProducts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pagedProducts, totalCount);
    }



    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByGroupByHavingPaginationWithDapperAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                decimal? minPrice = null, decimal? maxPrice = null)
    {
        var offset = (page - 1) * pageSize;

        var queryBuilder = new StringBuilder();

        queryBuilder.Append(@"
        SELECT p1.Id, p1.Name, p1.Description, p1.Quantity, p1.Price, p1.CreatedOnUtc, p1.UpdatedOnUtc
        FROM Products p1
        INNER JOIN Products p2 ON p1.Name = p2.Name AND p1.Id <> p2.Id ");

        // Adding WHERE conditions dynamically
        queryBuilder.Append("WHERE 1=1 "); // Ensures any additional WHERE conditions can be appended safely

      

        if (minPrice.HasValue)
        {
            queryBuilder.Append("AND p1.Price >= @MinPrice ");
        }

        if (maxPrice.HasValue)
        {
            queryBuilder.Append("AND p1.Price <= @MaxPrice ");
        }

        // Optional GROUP BY and HAVING clauses
        queryBuilder.Append(@" GROUP BY p1.Id, p1.Name, p1.Description, p1.Quantity, p1.Price, p1.CreatedOnUtc, p1.UpdatedOnUtc HAVING COUNT(p2.Id) > 1 ");

        // Adding ORDER BY clause
        if (!string.IsNullOrEmpty(orderBy))
        {
            if (orderBy == "created_on")
            {
                queryBuilder.Append("ORDER BY p1.CreatedOnUtc ");
            }
            else if (orderBy == "price_desc")
            {
                queryBuilder.Append("ORDER BY p1.Price DESC ");
            }
            else if (orderBy == "price_asc")
            {
                queryBuilder.Append("ORDER BY p1.Price ASC ");
            }
        }

        // Adding pagination
        queryBuilder.Append("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

        // Count query for total records (without pagination)
        var countQuery = @"
        SELECT COUNT(1) 
        FROM Products p1 
        INNER JOIN Products p2 ON p1.Name = p2.Name AND p1.Id <> p2.Id
        WHERE 1=1 " +
            (minPrice.HasValue ? "AND p1.Price >= @MinPrice " : "") +
            (maxPrice.HasValue ? "AND p1.Price <= @MaxPrice " : "");

        using var connection = _databaseFactory.CreateDapperConnection();

        // Execute product query
        var products = await connection.QueryAsync<Product>(
            queryBuilder.ToString(),
            new { Offset = offset, PageSize = pageSize, MinPrice = minPrice, MaxPrice = maxPrice },
            commandTimeout: _timeoutInSeconds
        );

        // Execute count query
        var totalRecords = await connection.ExecuteScalarAsync<int>(
            countQuery,
            new { MinPrice = minPrice, MaxPrice = maxPrice },
            commandTimeout: _timeoutInSeconds
        );

        return (products.ToList(), totalRecords);
    }


    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterOrderByGroupByHavingPaginationWithEFCoreAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                 decimal? minPrice = null, decimal? maxPrice = null)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        // Base query using self-join with LINQ
        var query = from p1 in context.Set<Product>()
                    join p2 in context.Set<Product>() on p1.Name equals p2.Name
                    where p1.Id != p2.Id // Avoid self-matching by ID
                    select p1;

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Grouping by product fields
        var groupedQuery = query
            .GroupBy(p => new { p.Id, p.Name, p.Description, p.Quantity, p.Price, p.CreatedOnUtc, p.UpdatedOnUtc })
            .Where(g => g.Count() > 1) // HAVING condition equivalent
            .Select(g => new Product
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Description = g.Key.Description,
                Quantity = g.Key.Quantity,
                Price = g.Key.Price,
                CreatedOnUtc = g.Key.CreatedOnUtc,
                UpdatedOnUtc = g.Key.UpdatedOnUtc
            }); // Select grouped fields

        // Ordering results dynamically
        if (!string.IsNullOrEmpty(orderBy))
        {
            groupedQuery = orderBy switch
            {
                "created_on" => groupedQuery.OrderBy(p => p.CreatedOnUtc),
                "price_desc" => groupedQuery.OrderByDescending(p => p.Price),
                "price_asc" => groupedQuery.OrderBy(p => p.Price),
                _ => groupedQuery
            };
        }

        // Fetching total record count before pagination
        var totalCount = await groupedQuery.CountAsync();

        // Applying pagination
        var pagedProducts = await groupedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pagedProducts, totalCount);
    }

    public async Task<(IReadOnlyList<Product?>, int TotalRecords)> FindWithFilterWithOrderByGroupByHavingPaginationWithEFCoreAsNoTrackingAsync(string orderBy, int page = 1, int pageSize = 10,
                                                                                                 decimal? minPrice = null, decimal? maxPrice = null)
    {
        await using var context = _databaseFactory.CreateEFCoreContext();
        
        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_timeoutInSeconds));

        // Base query using self-join with LINQ
        var query = from p1 in context.Set<Product>().AsNoTracking()
                    join p2 in context.Set<Product>().AsNoTracking() on p1.Name equals p2.Name
                    where p1.Id != p2.Id // Avoid self-matching by ID
                    select p1;

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Grouping by product fields
        var groupedQuery = query
            .GroupBy(p => new { p.Id, p.Name, p.Description, p.Quantity, p.Price, p.CreatedOnUtc, p.UpdatedOnUtc })
            .Where(g => g.Count() > 1) // HAVING condition equivalent
            .Select(g => new Product
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Description = g.Key.Description,
                Quantity = g.Key.Quantity,
                Price = g.Key.Price,
                CreatedOnUtc = g.Key.CreatedOnUtc,
                UpdatedOnUtc = g.Key.UpdatedOnUtc
            }); // Select grouped fields

        // Ordering results dynamically
        if (!string.IsNullOrEmpty(orderBy))
        {
            groupedQuery = orderBy switch
            {
                "created_on" => groupedQuery.OrderBy(p => p.CreatedOnUtc),
                "price_desc" => groupedQuery.OrderByDescending(p => p.Price),
                "price_asc" => groupedQuery.OrderBy(p => p.Price),
                _ => groupedQuery
            };
        }

        // Fetching total record count before pagination
        var totalCount = await groupedQuery.CountAsync();

        // Applying pagination
        var pagedProducts = await groupedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pagedProducts, totalCount);
    }

}
