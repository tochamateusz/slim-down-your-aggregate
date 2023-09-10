using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PublishingHouse.Persistence;

namespace PublishingHouse.Api.Tests;

public class ApiSpecification: ApiSpecification<Program>
{
    public ApiSpecification(): base(new TestWebApplicationFactory()) { }
}

public class TestWebApplicationFactory: WebApplicationFactory<Program>
{
    private readonly string schemaName;

    public TestWebApplicationFactory(): this(Guid.NewGuid().ToString("N").ToLower()) { }

    public TestWebApplicationFactory(string schemaName) =>
        this.schemaName = schemaName;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services
                .AddTransient(s =>
                {
                    var connectionString = s.GetRequiredService<IConfiguration>()
                        .GetConnectionString("PublishingHouse");
                    var options = new DbContextOptionsBuilder<PublishingHouseDbContext>();
                    options.UseNpgsql(
                        $"{connectionString}; searchpath = {schemaName.ToLower()}",
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", schemaName.ToLower()));
                    return options.Options;
                });
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PublishingHouseDbContext>().Database;
        database.ExecuteSqlRaw("TRUNCATE TABLE \"Books\" CASCADE");

        return host;
    }
}
