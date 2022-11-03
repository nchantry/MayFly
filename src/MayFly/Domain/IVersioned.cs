namespace MayFly.Domain;

public interface IVersioned
{
    long Version { get; set; }
}