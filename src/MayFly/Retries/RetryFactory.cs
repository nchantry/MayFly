using MayFly.Retries.Internal;

namespace MayFly.Retries;

public static class RetryFactory
{
    public static IRetryBuilder Create()
    {
        return new RetryBuilder();
    }
}