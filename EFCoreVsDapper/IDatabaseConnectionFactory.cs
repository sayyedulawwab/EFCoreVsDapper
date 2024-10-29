using System.Data;

namespace EFCoreVsDapper;
public interface IDatabaseConnectionFactory
{
    IDbConnection CreateDapperConnection();
    ApplicationDbContext CreateEFCoreContext();
}
