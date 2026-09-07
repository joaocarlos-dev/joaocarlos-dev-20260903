using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Authentication;

public sealed class AuthenticationQueryTests
{
    [Fact]
    public async Task Authenticate_WithValidCredentials_ShouldReturnToken()
    {
        var repository = new FakeUserRepository();
        var user = new User("USR-001", "admin", "hashed:password123");
        repository.Users.Add(user);
        await using var provider = CreateProvider(repository);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new AuthenticateQuery("admin", "password123");

        var result = await mediator.Send(query);

        Assert.Equal($"token:{user.Id}", result.AccessToken);
    }

    [Fact]
    public async Task Authenticate_ShouldTrimLoginButPreservePassword()
    {
        var repository = new FakeUserRepository();
        var user = new User("USR-001", "admin", "hashed:pass word");
        repository.Users.Add(user);
        await using var provider = CreateProvider(repository);
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new AuthenticateQuery(" admin ", "pass word"));

        Assert.Equal($"token:{user.Id}", result.AccessToken);
    }

    [Fact]
    public async Task Authenticate_WithInvalidPassword_ShouldThrowAuthenticationException()
    {
        var repository = new FakeUserRepository();
        repository.Users.Add(new User("USR-001", "admin", "hashed:password123"));
        await using var provider = CreateProvider(repository);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new AuthenticateQuery("admin", "wrong-password");

        var action = () => mediator.Send(query);

        await Assert.ThrowsAsync<AuthenticationException>(action);
    }

    [Fact]
    public async Task Authenticate_WithMissingUser_ShouldThrowAuthenticationException()
    {
        var repository = new FakeUserRepository();
        await using var provider = CreateProvider(repository);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new AuthenticateQuery("missing", "password123");

        var action = () => mediator.Send(query);

        await Assert.ThrowsAsync<AuthenticationException>(action);
    }

    [Fact]
    public async Task Authenticate_WithInactiveUser_ShouldRejectAuthentication()
    {
        var repository = new FakeUserRepository();
        var user = new User("USR-001", "admin", "hashed:password123");
        user.ChangeStatus(EntityStatus.Inactive);
        repository.Users.Add(user);
        await using var provider = CreateProvider(repository);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new AuthenticateQuery("admin", "password123");

        var action = () => mediator.Send(query);

        await Assert.ThrowsAsync<AuthenticationException>(action);
    }

    [Fact]
    public async Task Authenticate_WhenAccountRateLimited_ShouldThrowTooManyRequestsException()
    {
        var repository = new FakeUserRepository();
        repository.Users.Add(new User("USR-001", "admin", "hashed:password123"));
        var limiter = new FakeLoginAttemptLimiter { Allowed = false };
        await using var provider = CreateProvider(repository, limiter);
        var mediator = provider.GetRequiredService<IMediator>();

        var action = () => mediator.Send(new AuthenticateQuery("admin", "password123"));

        await Assert.ThrowsAsync<TooManyRequestsException>(action);
    }

    private static ServiceProvider CreateProvider(
        FakeUserRepository repository,
        FakeLoginAttemptLimiter? limiter = null)
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        services.AddLogging();
        services.AddSingleton<IUserRepository>(repository);
        services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
        services.AddSingleton<ITokenService, FakeTokenService>();
        services.AddSingleton<ILoginAttemptLimiter>(limiter ?? new FakeLoginAttemptLimiter());

        return services.BuildServiceProvider();
    }
}
