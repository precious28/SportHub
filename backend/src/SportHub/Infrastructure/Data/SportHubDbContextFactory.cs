using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportHub.Infrastructure.Data;

public class SportHubDbContextFactory : IDesignTimeDbContextFactory<SportHubDbContext>
{
    public SportHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SportHubDbContext>()
            .UseMySql("Server=localhost;Database=sporthub;User=root;Password=root;",
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;
        return new SportHubDbContext(options);
    }
}
