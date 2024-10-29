//setup DI
using BenchmarkDotNet.Running;
using EFCoreVsDapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


var connectionString = @"Server=(localdb)\mssqllocaldb;Database=TestEcomDB;Trusted_Connection=True;";

var serviceProvider = new ServiceCollection()
    .AddDbContext<ApplicationDbContext>(builder => builder.UseSqlServer(connectionString))
    .AddTransient<IDatabaseConnectionFactory>(_ => new DatabaseConnectionFactory(connectionString))
    .AddTransient<IProductRepository, ProductRepository>()
    .BuildServiceProvider();


// do actual work here
var productRepository = serviceProvider.GetService<IProductRepository>();

await productRepository.DeleteAllAsync();
await productRepository.InsertAsync();

var summary = BenchmarkRunner.Run<BenchmarkEFCoreDapper>();