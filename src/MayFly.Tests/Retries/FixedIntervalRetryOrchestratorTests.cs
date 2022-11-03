using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using MayFly.Retries;
using NSubstitute;
using Xunit;

namespace MayFly.Tests.Retries;

public class FixedIntervalRetryOrchestratorTests
{
    [Fact]
    public void at_least_one_interval_is_required()
    {
        var act1 = () => new FixedIntervalRetryOrchestrator();
        var act2 = () => new FixedIntervalRetryOrchestrator(null!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task intervals_are_observed()
    {
        var tolerance = TimeSpan.FromSeconds(1);
        var started = DateTime.Now;
        var intervals = new[] { TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.75), TimeSpan.FromSeconds(1) };
        var sum = TimeSpan.FromSeconds(intervals.Select(x => x.TotalSeconds).Sum());
        var subject = new FixedIntervalRetryOrchestrator(intervals);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        for ( var i = 0; i < intervals.Length; i++)
        {
            var retryContext = new RetryContext(i + 1, started, DateTime.Now, new Exception());
            subject.ShouldWeTryAgain(retryContext).Should().Be(true);
            await subject.GetPrerequisiteForNextTry(retryContext);
        }
        stopwatch.Elapsed.Add(tolerance).Should().BeGreaterOrEqualTo(sum);
    }
    
    [Fact]
    public void out_of_range_retries_return_completed_task()
    {
        var started = DateTime.Now;
        var subject = new FixedIntervalRetryOrchestrator(TimeSpan.FromSeconds(10));

        var retryContext = new RetryContext(2, started, DateTime.Now, new Exception());
        subject.ShouldWeTryAgain(retryContext).Should().Be(false);
        var task = subject.GetPrerequisiteForNextTry(retryContext);
        task.Should().Be(Task.CompletedTask);
    }

    [Fact]
    public void orchestrator_can_be_configured_via_extension()
    {
        var retryBuilder = Substitute.For<IRetryBuilder>();

        retryBuilder.UseFixedIntervalRetryOrchestrator(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

        retryBuilder.Received(1).UseOrchestrator(Arg.Any<FixedIntervalRetryOrchestrator>());
    }
    
    
}