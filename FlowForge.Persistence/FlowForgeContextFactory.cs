using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FlowForge.Persistence
{
    public class FlowForgeContextFactory : IDesignTimeDbContextFactory<FlowForgeAPIDbContext>
    {
        public FlowForgeAPIDbContext CreateDbContext(string[] args)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "../FlowForge.API/");

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            IConfiguration configuration = configurationBuilder.SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<FlowForgeAPIDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new FlowForgeAPIDbContext(optionsBuilder.Options);
        }
    }
}