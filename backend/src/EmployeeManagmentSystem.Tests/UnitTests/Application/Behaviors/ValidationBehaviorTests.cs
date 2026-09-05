using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldInvokeNextBehavior()
    {
        var command = new TestCommand("valid");
        var validator = new TestCommandValidator();
        var behavior = new ValidationBehavior<TestCommand, string>([validator]);
        var wasCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(command.Value);
        };

        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.Equal(command.Value, result);
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowBeforeNextBehavior()
    {
        var command = new TestCommand(string.Empty);
        var validator = new TestCommandValidator();
        var behavior = new ValidationBehavior<TestCommand, string>([validator]);
        var wasCalled = false;
        RequestHandlerDelegate<string> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(command.Value);
        };

        Func<Task> action = () => behavior.Handle(command, next, CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.False(wasCalled);
    }

    [Fact]
    public async Task Handle_WithInvalidQuery_ShouldThrowBeforeNextBehavior()
    {
        var query = new TestQuery(Guid.Empty);
        var validator = new TestQueryValidator();
        var behavior = new ValidationBehavior<TestQuery, Guid>([validator]);
        var wasCalled = false;
        RequestHandlerDelegate<Guid> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(query.Id);
        };

        Func<Task> action = () => behavior.Handle(query, next, CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.False(wasCalled);
    }

    [Fact]
    public async Task Handle_WithoutValidators_ShouldInvokeNextBehavior()
    {
        var command = new TestCommand("valid");
        var behavior = new ValidationBehavior<TestCommand, string>([]);
        RequestHandlerDelegate<string> next = _ => Task.FromResult(command.Value);

        var result = await behavior.Handle(command, next, CancellationToken.None);

        Assert.Equal(command.Value, result);
    }

    private sealed record TestCommand(string Value) : ICommand<string>;

    private sealed record TestQuery(Guid Id) : IQuery<Guid>;

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Value).NotEmpty();
        }
    }

    private sealed class TestQueryValidator : AbstractValidator<TestQuery>
    {
        public TestQueryValidator()
        {
            RuleFor(query => query.Id).NotEmpty();
        }
    }
}
