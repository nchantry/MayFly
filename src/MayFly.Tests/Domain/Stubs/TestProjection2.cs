using System;
using MayFly.Domain;

namespace MayFly.Tests.Domain.Stubs;

public class TestProjection2 : IProjection
{
    public long Version { get; set; }
    public Guid Id { get; }
}