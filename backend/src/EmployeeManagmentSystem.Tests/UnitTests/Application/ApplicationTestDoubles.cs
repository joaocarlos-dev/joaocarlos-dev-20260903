using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using EmployeeManagmentSystem.Application.Common.Events;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application;

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => string.Equals(user.Login, login, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyCollection<User>> ListAsync(
        EntityStatus? status,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<User>>(
            Users.Where(user => !status.HasValue || user.Status == status.Value).ToArray());

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(user => user.Code == code));

    public Task<bool> LoginExistsAsync(string login, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(user => string.Equals(user.Login, login, StringComparison.OrdinalIgnoreCase)));

    public Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Count(user => user.Role == UserRole.Administrator));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }
}

internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    public List<Employee> Employees { get; } = [];
    public int ListByUnitIdsCalls { get; private set; }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Employees.SingleOrDefault(employee => employee.Id == id));

    public Task<IReadOnlyCollection<Employee>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Employee>>(Employees.Where(employee => !employee.IsDeleted).ToArray());

    public Task<IReadOnlyCollection<Employee>> ListByUnitIdsAsync(
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken = default)
    {
        ListByUnitIdsCalls++;
        return Task.FromResult<IReadOnlyCollection<Employee>>(
            Employees.Where(employee => unitIds.Contains(employee.UnitId) && !employee.IsDeleted).ToArray());
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Employees.Any(employee => employee.Code == code));

    public Task<bool> UserIsLinkedAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Employees.Any(employee => employee.UserId == userId));

    public Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        Employees.Add(employee);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitRepository : IUnitRepository
{
    public List<Unit> Units { get; } = [];

    public Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Units.SingleOrDefault(unit => unit.Id == id));

    public Task<IReadOnlyCollection<Unit>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Unit>>(Units.ToArray());

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Units.Any(unit => unit.Code == code));

    public Task AddAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        Units.Add(unit);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }

    public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IUnitOfWorkTransaction>(new FakeUnitOfWorkTransaction());

    public Task<IUnitOfWorkTransaction> BeginSerializableTransactionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IUnitOfWorkTransaction>(new FakeUnitOfWorkTransaction());

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCalls++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeUnitOfWorkTransaction : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
}

internal sealed class FakeTokenService : ITokenService
{
    public string Generate(User user) => $"token:{user.Id}";
}

internal sealed class FakeLoginAttemptLimiter : ILoginAttemptLimiter
{
    public bool Allowed { get; set; } = true;
    public int RetryAfterSeconds => 60;

    public Task<bool> IsAllowedAsync(string login, string? clientIp, CancellationToken cancellationToken = default) =>
        Task.FromResult(Allowed);
}

internal sealed class FakeOutboxRepository : IOutboxRepository
{
    public string WorkerId => "test-worker";
    public List<OutboxMessage> Messages { get; } = [];

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<OutboxMessage>>(Messages.Where(message => message.ProcessedAt is null).Take(batchSize).ToArray());

    public Task<bool> MarkProcessedAsync(Guid id, string workerId, CancellationToken cancellationToken = default)
    {
        Messages.Single(message => message.Id == id).ProcessedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> MarkFailedAsync(Guid id, string error, string workerId, CancellationToken cancellationToken = default)
    {
        var message = Messages.Single(item => item.Id == id);
        message.Attempts++;
        message.LastError = error;
        return Task.FromResult(true);
    }

    public Task PruneProcessedAsync(TimeSpan retention, int batchSize, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
