using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using MayFly.Retries;
using NSubstitute;
using Xunit;

namespace MayFly.Tests.Retries;

public class ExponentialBackoffRetryOrchestratorTests
{
    [Fact]
    public void inputs_are_guarded()
    {
        var act1 =  () => new ExponentialBackoffRetryOrchestrator(-1, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        var act2 =  () => new ExponentialBackoffRetryOrchestrator(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public async Task intervals_get_longer()
    {
        var maxRetries = 3;
        var started = DateTime.Now;
        var subject = new ExponentialBackoffRetryOrchestrator(maxRetries, TimeSpan.FromMilliseconds(50), TimeSpan.FromSeconds(100));

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        TimeSpan lastInterval = TimeSpan.Zero;

        for (var i = 0; i < maxRetries; i++)
        {
            stopWatch.Restart();
            var retryContext = new RetryContext(i + 1, started, DateTime.Now, new Exception());
            await subject.GetPrerequisiteForNextTry(retryContext);
            stopWatch.Elapsed.Should().BeGreaterThan(lastInterval);
        }
    }
    
    
    [Fact]
    public void retries_aborted_after_max_interval()
    {
        var started = DateTime.Now;
        var subject = new ExponentialBackoffRetryOrchestrator(99, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));

        subject.ShouldWeTryAgain(new RetryContext(1, started, started.AddSeconds(1), new Exception())).Should().Be(true);
        subject.ShouldWeTryAgain(new RetryContext(1, started, started.AddSeconds(2), new Exception())).Should().Be(true);
        subject.ShouldWeTryAgain(new RetryContext(1, started, started.AddSeconds(3), new Exception())).Should().Be(true);
        subject.ShouldWeTryAgain(new RetryContext(1, started, started.AddSeconds(4), new Exception())).Should().Be(true);
        subject.ShouldWeTryAgain(new RetryContext(1, started, started.AddSeconds(5), new Exception())).Should().Be(false);
    }
    
    [Fact]
    public void out_of_range_retries_return_completed_task()
    {
        var started = DateTime.Now;
        var subject = new ExponentialBackoffRetryOrchestrator(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));

        var retryContext = new RetryContext(2, started, DateTime.Now, new Exception());
        subject.ShouldWeTryAgain(retryContext).Should().Be(false);
        var task = subject.GetPrerequisiteForNextTry(retryContext);
        task.Should().Be(Task.CompletedTask);
    }
    
        
    [Fact]
    public void orchestrator_can_be_configured_via_extension()
    {
        var retryBuilder = Substitute.For<IRetryBuilder>();

        retryBuilder.UseExponentialBackoffRetryOrchestrator(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));

        retryBuilder.Received(1).UseOrchestrator(Arg.Any<ExponentialBackoffRetryOrchestrator>());
    }

}