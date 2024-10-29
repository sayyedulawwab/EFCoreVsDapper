using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EFCoreVsDapper;
public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    public IDbConnection CreateDapperConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();

        return connection;
    }

    public ApplicationDbContext CreateEFCoreContext()
    {
        return new ApplicationDbContext(GetOptions());
    }

    private DbContextOptions<ApplicationDbContext> GetOptions()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TestEcomDB;Trusted_Connection=True;");

        return builder.Options;
    }
}
