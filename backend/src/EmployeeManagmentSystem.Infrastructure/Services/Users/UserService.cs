using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Services.Users;

public sealed class UserService(EmployeeManagementDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Login == login, cancellationToken);

    public async Task<IReadOnlyCollection<User>> ListAsync(
        EntityStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(user => user.Status == status.Value);
        }

        return await query.OrderBy(user => user.Login).ToArrayAsync(cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Code == code, cancellationToken);

    public Task<bool> LoginExistsAsync(string login, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(user => user.Login == login, cancellationToken);

    public Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default) =>
        context.Users.CountAsync(user => user.Role == UserRole.Administrator, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }
}

public static class UserServiceDependencyInjection
{
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserService>();
        return services;
    }
}
