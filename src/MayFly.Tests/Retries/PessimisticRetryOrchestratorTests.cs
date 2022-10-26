using System.Threading.Tasks;
using FluentAssertions;
using MayFly.Retries;
using MayFly.Retries.Internal;
using Xunit;

namespace MayFly.Tests.Retries;

public class PessimisticRetryOrchestratorTests
{
    [Fact]
    public void pessimistic_checks_are_always_negative()
    {
        var retryContext = new RetryContext(0, default, default);
        var subject = new PessimisticRetryOrchestrator();

        subject.ShouldWeTryAgain(retryContext).Should().BeFalse();
    }

    [Fact]
    public void pessimistic_checks_always_return_completed_tasks()
    {
        var retryContext = new RetryContext(0, default, default);
        var subject = new PessimisticRetryOrchestrator();

        subject.GetPrerequisiteForNextTry(retryContext).Should().Be(Task.CompletedTask);
    }
}