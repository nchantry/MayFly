using System;
using FluentAssertions;
using MayFly.Retries;
using MayFly.Retries.Internal;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MayFly.Tests.Retries;

public class DelegatedTransientErrorDetectorTests
{
    [Fact]
    public void delegates_are_executed()
    {
        var detector = Substitute.For<Func<Exception, bool>>();

        detector.Invoke(Arg.Any<Exception>()).Returns(true);

        var subject = new DelegatedTransientErrorDetector(detector);

        subject.IsTransient(new Exception()).Should().Be(true);
        detector.Received(1).Invoke(Arg.Any<Exception>());

    }

    [Fact]
    public void delegates_can_be_created_via_extension()
    {
        var retryManager = (RetryManager)RetryFactory.Create().UseDetector(_ => true).Build();

        retryManager.Detectors.Count.Should().Be(1);
        retryManager.Detectors[0].Should().BeOfType<DelegatedTransientErrorDetector>();
    }
}