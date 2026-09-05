using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application;

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.SingleOrDefault(user => user.Login == login));

    public Task<IReadOnlyCollection<User>> ListAsync(
        EntityStatus? status,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<User>>(
            Users.Where(user => !status.HasValue || user.Status == status.Value).ToArray());

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(user => user.Code == code));

    public Task<bool> LoginExistsAsync(string login, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(user => user.Login == login));

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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCalls++;
        return Task.FromResult(1);
    }
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
