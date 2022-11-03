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

        Action act1 = () => builder.UseDetector((null as ITransientErrorDetector)!);
        Action act2 = () => builder.UseDetector((null as Type)!);
        Action act3 = () => builder.UseDetector(typeof(int));
        Action act4 = () => builder.UseDetectors((null as Type)!, (null as Type)!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentOutOfRangeException>();
        act4.Should().Throw<ArgumentNullException>();
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

        Action act1 = () => builder.UseOrchestrator((null as IRetryOrchestrator)!);
        Action act2 = () => builder.UseOrchestrator((null as Type)!);
        Action act3 = () => builder.UseOrchestrator(typeof(FixedIntervalRetryOrchestrator));
        Action act4 = () => builder.UseOrchestrator(typeof(int));
        Action act5 = () => builder.UseOrchestrator<FixedIntervalRetryOrchestrator>(() => null!);
        Action act6 = () => builder.UseOrchestrator<FixedIntervalRetryOrchestrator>(() => throw new Exception());
        Action act7 = () => builder.UseOrchestrator((Func<FixedIntervalRetryOrchestrator>)null!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentException>();
        act4.Should().Throw<ArgumentOutOfRangeException>();
        act5.Should().Throw<ArgumentOutOfRangeException>();
        act6.Should().Throw<ArgumentOutOfRangeException>();
        act7.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void use_orchestrator_guards_inputs2()
    {
        IRetryBuilder builder = null!;

        Action act1 = () => builder.UseOrchestrator((null as Type)!);
        Action act2 = () => builder.UseOrchestrator(typeof(FixedIntervalRetryOrchestrator));
        Action act3 = () => builder.UseOrchestrator(typeof(int));
        Action act4 = () => builder.UseOrchestrator<PessimisticRetryOrchestrator>();
        Action act5 = () => builder.UseOrchestrator<PessimisticRetryOrchestrator>(() => null!);
        Action act6 = () => builder.UseOrchestrator<PessimisticRetryOrchestrator>(() => throw new Exception());
        Action act7 = () => builder.UseOrchestrator((Func<PessimisticRetryOrchestrator>)null!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
        act5.Should().Throw<ArgumentNullException>();
        act6.Should().Throw<ArgumentNullException>();
        act7.Should().Throw<ArgumentNullException>();
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
        builder.UseOrchestrator(new PessimisticRetryOrchestrator());

        builder.Received(2).UseOrchestrator(Arg.Any<IRetryOrchestrator>());
    }

    [Fact]
    public void UseOrchestrator_generic_method_uses_factory_method()
    {
        var builder = Substitute.For<IRetryBuilder>();
        
        builder.UseOrchestrator(() => new PessimisticRetryOrchestrator());

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