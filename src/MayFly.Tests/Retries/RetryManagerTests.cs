using System;
using System.Threading.Tasks;
using FluentAssertions;
using MayFly.Retries;
using MayFly.Retries.Internal;
using NSubstitute;
using Xunit;

namespace MayFly.Tests.Retries;

public class RetryManagerTests
{
    [Fact]
    public void retry_manager_uses_detectors_and_orchestrators_sync_action()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(true);
        orchestrator.GetPrerequisiteForNextTry(Arg.Any<RetryContext>()).Returns(Task.CompletedTask);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        manager.Run((ctx) =>
        {
            if (ctx.Attempts < 3) throw new Exception();
        });

        detector.Received(3).IsTransient(Arg.Any<Exception>());
        orchestrator.Received(3).ShouldWeTryAgain(Arg.Any<RetryContext>());
        orchestrator.Received(3).GetPrerequisiteForNextTry(Arg.Any<RetryContext>());
    }
    
    [Fact]
    public void retry_manager_uses_detectors_and_orchestrators_sync_function()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(true);
        orchestrator.GetPrerequisiteForNextTry(Arg.Any<RetryContext>()).Returns(Task.CompletedTask);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        var result = manager.Run((ctx) =>
        {
            if (ctx.Attempts < 3) throw new Exception();
            return 1;
        });

        detector.Received(3).IsTransient(Arg.Any<Exception>());
        orchestrator.Received(3).ShouldWeTryAgain(Arg.Any<RetryContext>());
        orchestrator.Received(3).GetPrerequisiteForNextTry(Arg.Any<RetryContext>());
        result.Should().Be(1);
    }
    
    [Fact]
    public async Task retry_manager_uses_detectors_and_orchestrators_async_action()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(true);
        orchestrator.GetPrerequisiteForNextTry(Arg.Any<RetryContext>()).Returns(Task.CompletedTask);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        await manager.RunAsync(async (ctx) =>
        {
            await Task.CompletedTask;
            if (ctx.Attempts < 3) throw new Exception();
        });

        detector.Received(3).IsTransient(Arg.Any<Exception>());
        orchestrator.Received(3).ShouldWeTryAgain(Arg.Any<RetryContext>());
        await orchestrator.Received(3).GetPrerequisiteForNextTry(Arg.Any<RetryContext>())!;
    }
    
    [Fact]
    public async Task retry_manager_uses_detectors_and_orchestrators_async_function()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(true);
        orchestrator.GetPrerequisiteForNextTry(Arg.Any<RetryContext>()).Returns(Task.CompletedTask);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        var result = await manager.RunAsync(async (ctx) =>
        {
            await Task.CompletedTask;
            if (ctx.Attempts < 3) throw new Exception();
            return 1;
        });

        detector.Received(3).IsTransient(Arg.Any<Exception>());
        orchestrator.Received(3).ShouldWeTryAgain(Arg.Any<RetryContext>());
        await orchestrator.Received(3).GetPrerequisiteForNextTry(Arg.Any<RetryContext>())!;
        result.Should().Be(1);
    }

    
    [Fact]
    public void retry_manager_throws_when_errors_are_not_transient_sync()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(false);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        Action act = () => manager.Run((ctx) =>
        {
            if (ctx.Attempts < 3) throw new Exception();
        });

        act.Should().Throw<Exception>();
        detector.Received(1).IsTransient(Arg.Any<Exception>());
    }

        
    [Fact]
    public void retry_manager_throws_when_orchestrator_breaches_sync()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(false);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        var act = () => manager.Run((_) => throw new Exception());

        act.Should().Throw<Exception>();
        detector.Received(1).IsTransient(Arg.Any<Exception>());
        orchestrator.Received(1).ShouldWeTryAgain(Arg.Any<RetryContext>());
    }

    [Fact]
    public async Task retry_manager_throws_when_errors_are_not_transient_async()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        
        detector.IsTransient(Arg.Any<Exception>()).Returns(true);
        detector.IsTransient(Arg.Any<ApplicationException>()).Returns(false);
        orchestrator.ShouldWeTryAgain(Arg.Any<RetryContext>()).Returns(false);
        
        var manager = RetryFactory.Create().UseDetector(detector).UseOrchestrator(orchestrator).Build();

        var act1 = () => manager.RunAsync(async (_) =>
        {
            await Task.CompletedTask;
            throw new Exception();
        });

        var act2 = () => manager.RunAsync(async (_) =>
        {
            await Task.CompletedTask;
            throw new ApplicationException();
        });

        await act1.Should().ThrowAsync<Exception>();
        await act2.Should().ThrowAsync<ApplicationException>();
        detector.Received(2).IsTransient(Arg.Any<Exception>());
        detector.Received(1).IsTransient(Arg.Any<ApplicationException>());
        orchestrator.Received(1).ShouldWeTryAgain(Arg.Any<RetryContext>());
    }

    [Fact]
    public void pessimistic_handling_is_used_by_default()
    {
        RetryManager retryManager = (RetryManager)RetryFactory.Create().Build();
        retryManager.Orchestrator.Should().BeOfType<PessimisticRetryOrchestrator>();
        retryManager.Detectors.Count.Should().Be(1);
        retryManager.Detectors[0].Should().BeOfType<PessimisticTransientErrorDetector>();
    }
}