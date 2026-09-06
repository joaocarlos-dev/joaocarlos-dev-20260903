using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DatabaseInitializerIntegrationTests(EmployeeManagementApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task InitializeDatabaseAsync_WithPreviousInitialMigrationHistory_ShouldNotRecreateExistingTables()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeManagementDbContext>();
        await context.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20260905211712_InitialCreate';

            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260905172901_InitialCreate', '8.0.11')
            ON CONFLICT ("MigrationId") DO NOTHING;
            """);

        await scope.ServiceProvider.InitializeDatabaseAsync(new InitialUserOptions());

        var appliedMigrations = await context.Database
            .SqlQueryRaw<string>(
                """
                SELECT "MigrationId" AS "Value"
                FROM "__EFMigrationsHistory"
                """)
            .ToArrayAsync();
        Assert.Contains("20260905211712_InitialCreate", appliedMigrations);
    }
}
