using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const long InitializationLockId = 1_164_591_772_469_447_505;

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
