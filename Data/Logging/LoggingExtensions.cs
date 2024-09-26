using System.Net;
using Microsoft.Extensions.Logging;

namespace Data.Logging;

internal static class LoggerExtensions
{
    public static void LogStatistics(this ILogger logger,
                                     string methodName,
                                     string typeName,
                                     string partitionKey,
                                     HttpStatusCode statusCode,
                                     long elapsedMs,
                                     double requestCharge)
    {
        logger.LogInformation("{MethodName}: {Type} {PartitionKey} returned {StatusCode} in {Time}ms. Request charge: {RequestCharge} RUs.",
                           methodName,
                           typeName,
                           partitionKey,
                           statusCode,
                           elapsedMs,
                           requestCharge
        );
    }

    public static void LogStatistics(this ILogger logger,
                                     string methodName,
                                     string typeName,
                                     string partitionKey,
                                     HttpStatusCode statusCode,
                                     long elapsedMs)
    {
        logger.LogInformation("{MethodName}: {Type} {PartitionKey} returned {StatusCode} in {Time}ms.",
                              methodName,
                              typeName,
                              partitionKey,
                              statusCode,
                              elapsedMs
        );
    }
}