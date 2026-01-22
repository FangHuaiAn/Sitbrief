using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sitbrief.Infrastructure.Data;

public class SitbriefDbContextFactory : IDesignTimeDbContextFactory<SitbriefDbContext>
{
    public SitbriefDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SitbriefDbContext>();

        // Use SQLite with a design-time connection string
        optionsBuilder.UseSqlite("Data Source=sitbrief.db");

        return new SitbriefDbContext(optionsBuilder.Options);
    }
}
