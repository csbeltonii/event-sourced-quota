using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.QuotaTables.Behaviors;

public class ExecutionTimeLoggingBehavior<TRequest, TResponse>(ILogger<ExecutionTimeLoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    private readonly string _className = nameof(ExecutionTimeLoggingBehavior<TRequest, TResponse>);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var identifier = Guid.NewGuid();

        TResponse response;

        logger.LogInformation(
            "{ClassName}: Executing {RequestType} {RequestIdentifier}.",
            _className,
             GetCommandFriendlyName(typeof(TRequest).Name),
            identifier);

        try
        {
            response = await next();
        }
        finally
        {
            logger.LogInformation("{ClassName}: {RequestType} {RequestIdentifier} execution finished in {Time}ms",
                                _className,
                                GetCommandFriendlyName(typeof(TRequest).Name),
                                identifier,
                                stopwatch.ElapsedMilliseconds.ToString("##,###"));
        }

        return response;
    }

    private static string GetCommandFriendlyName(string name) => name.Split('.').Last();
}