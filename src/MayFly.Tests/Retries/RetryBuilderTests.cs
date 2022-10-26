using System;
using FluentAssertions;
using MayFly.Retries;
using MayFly.Retries.Internal;
using NSubstitute;
using Xunit;

namespace MayFly.Tests.Retries;

public class RetryBuilderTests
{
    [Fact]
    public void UseDetector_guards_inputs()
    {
        var builder = RetryFactory.Create();

        Action act1 = () => builder.UseDetector(null as ITransientErrorDetector);
        Action act2 = () => builder.UseDetector(null as Type);
        Action act3 = () => builder.UseDetector(typeof(int));

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UseDetector_instance_method_records_detector()
    {
        var detector = Substitute.For<ITransientErrorDetector>();
        var builder = (RetryBuilder)RetryFactory.Create();

        builder.UseDetector(detector);
        builder.Context.Detectors.Count.Should().Be(1);
        builder.Context.Detectors[0].Should().Be(detector);
    }

    [Fact]
    public void UseDetector_generic_method_uses_instance_method()
    {
        var builder = Substitute.For<IRetryBuilder>();
        builder.UseDetector<PessimisticTransientErrorDetector>();
        builder.Received(1).UseDetector(Arg.Any<ITransientErrorDetector>());
    }

    [Fact]
    public void UseDetector_type_method_uses_instance_method()
    {
        var builder = Substitute.For<IRetryBuilder>();
        builder.UseDetector(typeof(PessimisticTransientErrorDetector));
        builder.Received(1).UseDetector(Arg.Any<PessimisticTransientErrorDetector>());
    }

    [Fact]
    public void use_orchestrator_guards_inputs()
    {
        var builder = RetryFactory.Create();

        Action act1 = () => builder.UseOrchestrator(null as IRetryOrchestrator);
        Action act2 = () => builder.UseOrchestrator(null as Type);
        Action act3 = () => builder.UseOrchestrator(typeof(FixedIntervalRetryOrchestrator));
        Action act4 = () => builder.UseOrchestrator(typeof(int));

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentOutOfRangeException>();
        act4.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void UseOrchestrator_instance_method_records_orchestrator()
    {
        var orchestrator = Substitute.For<IRetryOrchestrator>();
        var builder = (RetryBuilder)RetryFactory.Create();
        
        builder.UseOrchestrator(orchestrator);
        builder.Context.Should().NotBeNull();
        builder.Context.Orchestrator.Should().NotBeNull();
        builder.Context.Orchestrator!.Equals(orchestrator).Should().Be(true);
    }


    [Fact]
    public void UseOrchestrator_generic_method_uses_instance_method()
    {
        var builder = Substitute.For<IRetryBuilder>();
        
        builder.UseOrchestrator<PessimisticRetryOrchestrator>();

        builder.Received(1).UseOrchestrator(Arg.Any<IRetryOrchestrator>());
    }


    [Fact]
    public void UseOrchestrator_type_method_uses_instance_method()
    {
        var builder = Substitute.For<IRetryBuilder>();
        builder.UseOrchestrator(typeof(PessimisticRetryOrchestrator));
        builder.Received(1).UseOrchestrator(Arg.Any<PessimisticRetryOrchestrator>());
    }
}