using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Events;
using EmployeeManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public sealed class EmployeeManagementDbContext(DbContextOptions<EmployeeManagementDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<LoginRateLimitBucket> LoginRateLimitBuckets => Set<LoginRateLimitBucket>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default) =>
        new DbContextUnitOfWorkTransaction(
            await Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken));

    public async Task<IUnitOfWorkTransaction> BeginSerializableTransactionAsync(
        CancellationToken cancellationToken = default) =>
        new DbContextUnitOfWorkTransaction(
            await Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeManagementDbContext).Assembly);
    }
}
