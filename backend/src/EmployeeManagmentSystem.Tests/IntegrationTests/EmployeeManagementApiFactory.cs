using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<EmployeeManagementApiFactory>
{
    public const string Name = "Integration tests";
}

public sealed class EmployeeManagementApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string InitialUserCode = "ADM-001";
    private const string InitialUserLogin = "admin.integration";
    private const string InitialUserPassword = "Password123!";
    private const string JwtSigningKey = "integration-tests-signing-key-with-more-than-32-characters";

    private PostgreSqlContainer? container;
    private string connectionString = string.Empty;
    private readonly Dictionary<string, string?> previousEnvironmentVariables = [];

    public string AdminLogin => InitialUserLogin;

    public string AdminPassword => InitialUserPassword;

    public async Task InitializeAsync()
    {
        connectionString = CreateConnectionStringFromEnvironment();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("employee_management_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await container.StartAsync();
            connectionString = container.GetConnectionString();
        }

        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        SetEnvironmentVariable("Jwt__Issuer", "EmployeeManagmentSystem.IntegrationTests");
        SetEnvironmentVariable("Jwt__Audience", "EmployeeManagmentSystem.IntegrationTests.Client");
        SetEnvironmentVariable("Jwt__SigningKey", JwtSigningKey);
        SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        SetEnvironmentVariable("InitialUser__Code", InitialUserCode);
        SetEnvironmentVariable("InitialUser__Login", InitialUserLogin);
        SetEnvironmentVariable("InitialUser__Password", InitialUserPassword);

        await RecreateDatabaseAsync();
        using var client = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await DropDatabaseAsync();

        if (container is not null)
        {
            await container.DisposeAsync();
        }

        foreach (var variable in previousEnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await ClearDatabaseAsync();

        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.InitializeDatabaseAsync(
            new InitialUserOptions
            {
                Code = InitialUserCode,
                Login = InitialUserLogin,
                Password = InitialUserPassword
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
    }

    private static string CreateConnectionStringFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new NpgsqlConnectionStringBuilder(value)
        {
            Database = $"employee_management_tests_{Guid.NewGuid():N}"
        };

        return builder.ConnectionString;
    }

    private async Task DropDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await DropDatabaseCoreAsync();
    }

    private void SetEnvironmentVariable(string name, string value)
    {
        previousEnvironmentVariables.TryAdd(name, Environment.GetEnvironmentVariable(name));
        Environment.SetEnvironmentVariable(name, value);
    }

    private async Task RecreateDatabaseAsync()
    {
        await DropDatabaseCoreAsync();

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = GetDatabaseName(builder);
        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {QuoteIdentifier(database)}";
        await command.ExecuteNonQueryAsync();
        NpgsqlConnection.ClearAllPools();
    }

    private async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "TRUNCATE TABLE employees, units, users, login_rate_limit_buckets, outbox_messages RESTART IDENTITY CASCADE";
        await command.ExecuteNonQueryAsync();
    }

    private async Task DropDatabaseCoreAsync()
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = GetDatabaseName(builder);
        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var terminateCommand = connection.CreateCommand();
        terminateCommand.CommandText = """
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = $1 AND pid <> pg_backend_pid()
            """;
        terminateCommand.Parameters.AddWithValue(database);
        await terminateCommand.ExecuteNonQueryAsync();

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(database)}";
        await dropCommand.ExecuteNonQueryAsync();
        NpgsqlConnection.ClearAllPools();
    }

    private static string QuoteIdentifier(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string GetDatabaseName(NpgsqlConnectionStringBuilder builder) =>
        string.IsNullOrWhiteSpace(builder.Database)
            ? throw new InvalidOperationException("A database name is required for integration tests.")
            : builder.Database;
}
