using FluentAssertions;
using MayFly.Retries;
using Xunit;

namespace MayFly.Tests.Retries;

public class RetryFactoryTests
{
    [Fact]
    public void factories_create_builders()
    {
        var builder = RetryFactory.Create();

        builder.Should().NotBeNull();
    }
}