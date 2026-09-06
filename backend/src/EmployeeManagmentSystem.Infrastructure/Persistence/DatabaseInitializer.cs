using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const long InitializationLockId = 1_164_591_772_469_447_505;
    private const string CurrentInitialMigrationId = "20260905211712_InitialCreate";
    private const string PreviousInitialMigrationId = "20260905172901_InitialCreate";
    private const string EfCoreProductVersion = "8.0.11";

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        InitialUserOptions initialUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(initialUser);

        if (initialUser.IsConfigured)
        {
            initialUser.Validate();
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeManagementDbContext>();
        await context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_lock({InitializationLockId})",
                cancellationToken);

            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"""
                    CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                        "MigrationId" character varying(150) NOT NULL,
                        "ProductVersion" character varying(32) NOT NULL,
                        CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                    );

                    DO $$
                    BEGIN
                        IF to_regclass('public."__EFMigrationsHistory"') IS NOT NULL
                            AND to_regclass('public.units') IS NOT NULL
                            AND to_regclass('public.users') IS NOT NULL
                            AND to_regclass('public.employees') IS NOT NULL
                            AND EXISTS (
                                SELECT 1 FROM "__EFMigrationsHistory"
                                WHERE "MigrationId" = '{PreviousInitialMigrationId}'
                            )
                            AND NOT EXISTS (
                                SELECT 1 FROM "__EFMigrationsHistory"
                                WHERE "MigrationId" = '{CurrentInitialMigrationId}'
                            )
                        THEN
                            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                            VALUES ('{CurrentInitialMigrationId}', '{EfCoreProductVersion}');
                        END IF;
                    END $$;
                    """,
                    cancellationToken);

                await context.Database.MigrateAsync(cancellationToken);

                if (!initialUser.IsConfigured)
                {
                    return;
                }

                if (await context.Users.AnyAsync(user => user.Login == initialUser.Login.Trim(), cancellationToken))
                {
                    return;
                }

                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                var user = new User(
                    initialUser.Code,
                    initialUser.Login,
                    passwordHasher.Hash(initialUser.Password!));
                await context.Users.AddAsync(user, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_unlock({InitializationLockId})",
                    CancellationToken.None);
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
