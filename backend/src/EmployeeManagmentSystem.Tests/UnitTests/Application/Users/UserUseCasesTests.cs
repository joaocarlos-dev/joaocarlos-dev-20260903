using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.Users;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Users;

public sealed class UserUseCasesTests
{
    [Fact]
    public async Task CreateUser_WithUniqueData_ShouldHashPasswordAndPersistUser()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-001", "admin", "password123");

        var id = await mediator.Send(command);

        var user = Assert.Single(repository.Users);
        Assert.Equal(id, user.Id);
        Assert.Equal("hashed:password123", user.PasswordHash);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateLogin_ShouldThrowConflictAndNotPersist()
    {
        var repository = new FakeUserRepository();
        repository.Users.Add(new User("USR-001", "admin", "hashed:password123"));
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-002", " admin ", "password456");

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ApplicationConflictException>(action);
        Assert.Single(repository.Users);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task ListUsers_WithInactiveFilter_ShouldReturnOnlyInactiveUsers()
    {
        var repository = new FakeUserRepository();
        var activeUser = new User("USR-001", "active", "hashed:password123");
        var inactiveUser = new User("USR-002", "inactive", "hashed:password456");
        inactiveUser.ChangeStatus(EntityStatus.Inactive);
        repository.Users.AddRange([activeUser, inactiveUser]);
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new ListUsersQuery(EntityStatus.Inactive);

        var result = await mediator.Send(query);

        var user = Assert.Single(result);
        Assert.Equal(inactiveUser.Id, user.Id);
    }

    private static ServiceProvider CreateProvider(FakeUserRepository repository, FakeUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        services.AddSingleton<IUserRepository>(repository);
        services.AddSingleton<IUnitOfWork>(unitOfWork);
        services.AddSingleton<IPasswordHasher, FakePasswordHasher>();

        return services.BuildServiceProvider();
    }
}
