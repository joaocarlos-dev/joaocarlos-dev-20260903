using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Commands.Users.CreateUser;
using EmployeeManagmentSystem.Application.Commands.Users.UpdateUser;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.Queries.Users.GetUser;
using EmployeeManagmentSystem.Application.Queries.Users.GetUsers;
using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Users;

public sealed class UserOperationsTests
{
    [Fact]
    public async Task CreateUser_WithUniqueData_ShouldHashPasswordAndPersistUser()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var outbox = new FakeOutboxRepository();
        await using var provider = CreateProvider(repository, unitOfWork, outbox);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-001", "admin", "password123", EntityStatus.Active);

        var id = await mediator.Send(command);

        var user = Assert.Single(repository.Users);
        Assert.Equal(id, user.Id);
        Assert.Equal("hashed:password123", user.PasswordHash);
        Assert.Equal(EntityStatus.Active, user.Status);
        var message = Assert.Single(outbox.Messages);
        Assert.Equal("UserRegisteredEvent", message.Type);
        Assert.Contains(user.Id.ToString(), message.Payload);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_500Registrations_ShouldCreateAnOutboxMessageForEachUser()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var outbox = new FakeOutboxRepository();
        await using var provider = CreateProvider(repository, unitOfWork, outbox);
        var mediator = provider.GetRequiredService<IMediator>();

        for (var index = 1; index <= 500; index++)
        {
            await mediator.Send(new CreateUserCommand(
                $"USR-{index:000}",
                $"employee-{index:000}",
                "Password123!",
                EntityStatus.Active));
        }

        Assert.Equal(500, repository.Users.Count);
        Assert.Equal(500, outbox.Messages.Count);
        Assert.All(outbox.Messages, message => Assert.Equal("UserRegisteredEvent", message.Type));
    }

    [Fact]
    public async Task CreateUser_WithDuplicateLogin_ShouldThrowConflictAndNotPersist()
    {
        var repository = new FakeUserRepository();
        repository.Users.Add(new User("USR-001", "admin", "hashed:password123"));
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-002", " ADMIN ", "password456", EntityStatus.Active);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ApplicationConflictException>(action);
        Assert.Single(repository.Users);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_WithInactiveStatus_ShouldPersistInactiveUser()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-001", "admin", "password123", EntityStatus.Inactive);

        var id = await mediator.Send(command);

        var user = Assert.Single(repository.Users);
        Assert.Equal(id, user.Id);
        Assert.Equal(EntityStatus.Inactive, user.Status);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_WithInvalidStatus_ShouldFailValidationAndNotPersist()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand("USR-001", "admin", "password123", (EntityStatus)999);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Empty(repository.Users);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_WithLoginExceedingMaximumLength_ShouldFailValidationAndNotPersist()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUserCommand(
            "USR-001",
            new string('a', EntityFieldLengths.Login + 1),
            "password123",
            EntityStatus.Active);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Empty(repository.Users);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUser_WithTrimmedValuesAtMaximumLength_ShouldPersistNormalizedValues()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var code = new string('c', EntityFieldLengths.Code);
        var login = new string('l', EntityFieldLengths.Login);
        var command = new CreateUserCommand($" {code} ", $" {login} ", "password123", EntityStatus.Active);

        await mediator.Send(command);

        var user = Assert.Single(repository.Users);
        Assert.Equal(code, user.Code);
        Assert.Equal(login, user.Login);
        Assert.Equal(1, unitOfWork.SaveCalls);
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
        var query = new GetUsersQuery(EntityStatus.Inactive);

        var result = await mediator.Send(query);

        var user = Assert.Single(result);
        Assert.Equal(inactiveUser.Id, user.Id);
    }

    [Fact]
    public async Task GetUser_WithExistingUser_ShouldReturnUser()
    {
        var repository = new FakeUserRepository();
        var existingUser = new User("USR-001", "admin", "hashed:password123");
        repository.Users.Add(existingUser);
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new GetUserQuery(existingUser.Id);

        var result = await mediator.Send(query);

        Assert.Equal(existingUser.Id, result.Id);
        Assert.Equal(existingUser.Login, result.Login);
    }

    [Fact]
    public async Task GetUser_WithMissingUser_ShouldThrowNotFound()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new GetUserQuery(Guid.NewGuid());

        var action = () => mediator.Send(query);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task UpdateUser_WithExistingUser_ShouldUpdatePasswordAndStatus()
    {
        var repository = new FakeUserRepository();
        var existingUser = new User("USR-001", "admin", "hashed:password123");
        repository.Users.Add(existingUser);
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new UpdateUserCommand(existingUser.Id, "new-password", EntityStatus.Inactive);

        await mediator.Send(command);

        Assert.Equal("hashed:new-password", existingUser.PasswordHash);
        Assert.Equal(EntityStatus.Inactive, existingUser.Status);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task UpdateUser_WithMissingUser_ShouldThrowNotFoundAndNotPersist()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(repository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new UpdateUserCommand(Guid.NewGuid(), "new-password", EntityStatus.Inactive);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    private static ServiceProvider CreateProvider(
        FakeUserRepository repository,
        FakeUnitOfWork unitOfWork,
        FakeOutboxRepository? outbox = null)
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        services.AddSingleton<IUserRepository>(repository);
        services.AddSingleton<IUnitOfWork>(unitOfWork);
        services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
        services.AddSingleton<IOutboxRepository>(outbox ?? new FakeOutboxRepository());

        return services.BuildServiceProvider();
    }
}
