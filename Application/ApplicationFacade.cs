using Application.QuotaTables.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace Application;

public  static class ApplicationFacade
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services.AddMediatR(
            mediatrConfig =>
            {
                mediatrConfig.RegisterServicesFromAssembly(typeof(ApplicationFacade).Assembly);
                mediatrConfig.AddBehavior<VerifyQuotaTableDoesNotExistBehavior>();
                mediatrConfig.AddOpenBehavior(typeof(ExecutionTimeLoggingBehavior<,>));
            }
        );
}