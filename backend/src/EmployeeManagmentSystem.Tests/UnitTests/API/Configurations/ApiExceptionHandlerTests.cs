using EmployeeManagmentSystem.API.Configurations.Errors;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EmployeeManagmentSystem.Tests.UnitTests.API.Configurations;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithValidationException_ShouldReturnValidationProblemDetails()
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();
        var exception = new ValidationException(
            new[] { new ValidationFailure("Login", "Login is required.") });

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(problemDetailsService.ProblemDetails);
        Assert.Equal("Validation failed", problemDetails.Title);
        Assert.Equal(new[] { "Login is required." }, problemDetails.Errors["Login"]);
    }

    [Fact]
    public async Task TryHandleAsync_WithUniqueConstraintViolation_ShouldReturnConflict()
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();
        var postgresException = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation);
        var exception = new DbUpdateException("Save failed.", postgresException);

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal("Conflict", problemDetailsService.ProblemDetails?.Title);
        Assert.Equal("The resource already exists.", problemDetailsService.ProblemDetails?.Detail);
    }

    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public async Task TryHandleAsync_WithKnownException_ShouldReturnMappedProblemDetails(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle)
    {
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(problemDetailsService);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
        Assert.NotNull(problemDetailsService.ProblemDetails);
        Assert.Equal(expectedStatusCode, problemDetailsService.ProblemDetails.Status);
        Assert.Equal(expectedTitle, problemDetailsService.ProblemDetails.Title);
        Assert.True(problemDetailsService.ProblemDetails.Extensions.ContainsKey("traceId"));
    }

    public static IEnumerable<object[]> ExceptionMappings()
    {
        yield return new object[] { new AuthenticationException(), StatusCodes.Status401Unauthorized, "Authentication failed" };
        yield return new object[] { new NotFoundException("User", Guid.NewGuid()), StatusCodes.Status404NotFound, "Resource not found" };
        yield return new object[] { new ApplicationConflictException("Conflict."), StatusCodes.Status409Conflict, "Conflict" };
        yield return new object[] { new InvalidOperationException("Internal."), StatusCodes.Status500InternalServerError, "Unexpected error" };
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetails? ProblemDetails { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            ProblemDetails = context.ProblemDetails;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            ProblemDetails = context.ProblemDetails;
            return ValueTask.CompletedTask;
        }
    }
}
