using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreVsDapper;

[MemoryDiagnoser(false)]
public class BenchmarkEFCoreDapper
{

    private readonly ServiceProvider serviceProvider = new ServiceCollection()
                    .AddDbContext<ApplicationDbContext>(builder => builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TestEcomDB;Trusted_Connection=True;"))
                    .AddScoped<IDatabaseConnectionFactory>(_ => new DatabaseConnectionFactory(@"Server=(localdb)\mssqllocaldb;Database=TestEcomDB;Trusted_Connection=True;"))
                    .AddTransient<IProductRepository, ProductRepository>()
                    .BuildServiceProvider();

    private readonly IProductRepository _productRepository;

    public BenchmarkEFCoreDapper()
    {
        _productRepository = serviceProvider.GetService<IProductRepository>();
    }

    int numberOfIterations = 50;

    [Benchmark]
    public async Task GetByIdWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.GetByIdWithDapperAsync(2000);
        }
    }

    [Benchmark]
    public async Task GetByIdWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.GetByIdWithEFCoreAsync(2000);
        }
    }


    [Benchmark]
    public async Task GetByIdWithEFCoreAsNoTrackingAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.GetByIdWithEFCoreAsNoTrackingAsync(2000);
        }
    }

    [Benchmark]
    public async Task InsertProductWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.InsertProductWithDapperAsync("New product", "New product description .......... ", 50, 150, DateTime.UtcNow);
        }
    }

    [Benchmark]
    public async Task InsertProductWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.InsertProductWithEFCoreAsync("New product", "New product description .......... ", 50, 150, DateTime.UtcNow);
        }
    }


    [Benchmark]
    public async Task FindWithFilterOrderByPaginationWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterOrderByPaginationWithDapperAsync("price_desc", 1, 50, 75, 1000);
        }
    }

    [Benchmark]
    public async Task FindWithFilterOrderByPaginationWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterOrderByPaginationWithEFCoreAsync("price_desc", 1, 50, 75, 1000);
        }
    }

    [Benchmark]
    public async Task FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync("price_desc", 1, 50, 75, 1000);
        }
    }


    [Benchmark]
    public async Task FindWithFilterOrderByGroupByHavingPaginationWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterOrderByGroupByHavingPaginationWithDapperAsync("price_desc", 1, 50, 75, 1000);
        }
    }

    [Benchmark]
    public async Task FindWithFilterOrderByGroupByHavingPaginationWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterOrderByGroupByHavingPaginationWithEFCoreAsync("price_desc", 1, 50, 75, 1000);
        }
    }

    [Benchmark]
    public async Task FindWithFilterWithOrderByGroupByHavingPaginationWithEFCoreAsNoTrackingAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterWithOrderByGroupByHavingPaginationWithEFCoreAsNoTrackingAsync("price_desc", 1, 50, 75, 1000);
        }
    }
}

