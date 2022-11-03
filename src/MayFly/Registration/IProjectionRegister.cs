namespace MayFly.Registration;

public class ProjectionRegister
{
    public void RegisterProjection(Type aggregateType, Type projectionType)
    {
        if (aggregateType == null) throw new ArgumentNullException(nameof(aggregateType));
        if (projectionType == null) throw new ArgumentNullException(nameof(projectionType));

        throw new NotImplementedException();
    }
}
