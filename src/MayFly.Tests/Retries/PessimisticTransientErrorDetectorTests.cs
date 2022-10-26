using System;
using FluentAssertions;
using MayFly.Retries;
using MayFly.Retries.Internal;
using Xunit;

namespace MayFly.Tests.Retries;

public class PessimisticTransientErrorDetectorTests
{
    [Fact]
    public void pessimistic_detections_are_always_negative()
    {
        var subject = new PessimisticTransientErrorDetector();

        subject.IsTransient(new Exception()).Should().BeFalse();
    }
    
}