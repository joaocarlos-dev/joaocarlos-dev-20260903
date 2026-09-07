using EmployeeManagmentSystem.Infrastructure.Services.Outbox;

namespace EmployeeManagmentSystem.Tests.UnitTests.Infrastructure.Services.Outbox;

public sealed class RabbitMqOptionsTests
{
    [Fact]
    public void Validate_WithDefaults_ShouldSucceed()
    {
        var options = new RabbitMqOptions();

        options.Validate();
    }

    [Fact]
    public void Validate_WithInvalidQuarantineLimit_ShouldThrow()
    {
        var options = new RabbitMqOptions { UnroutedQueueMaxLength = 0 };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }
}
