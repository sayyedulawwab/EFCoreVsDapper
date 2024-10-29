using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreVsDapper;
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

    int numberOfIterations = 10;

    [Benchmark]
    public async Task GetByIdWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.GetByIdWithDapperAsync(50);
        }
    }

    [Benchmark]
    public async Task GetByIdWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.GetByIdWithEFCoreAsync(50);
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
    public async Task FindWithFilterWithDapperAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterWithDapperAsync("New", "price_desc", 1, 100);
        }
    }

    [Benchmark]
    public async Task FindWithFilterWithEFCoreAsync()
    {
        for (int i = 0; i < numberOfIterations; i++)
        {
            await _productRepository.FindWithFilterWithEFCoreAsync("New", "price_desc", 1, 100);
        }
    }
}

