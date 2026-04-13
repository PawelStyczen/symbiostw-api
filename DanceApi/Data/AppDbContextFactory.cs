using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DanceApi.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=symbiostw-db-dev;User Id=SA;Password=MyStrongPass123;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}
