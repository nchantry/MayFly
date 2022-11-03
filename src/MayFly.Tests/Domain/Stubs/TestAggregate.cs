using System;
using MayFly.Domain;

namespace MayFly.Tests.Domain.Stubs;

public class TestAggregate : Aggregate
{
    public TestAggregate(IServiceProvider serviceProvider, Guid aggregateId) : base(serviceProvider, aggregateId)
    {
    }
}