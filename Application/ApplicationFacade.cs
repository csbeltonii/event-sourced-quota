using Application.QuotaTables.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Application.Models;

namespace Application;

public  static class ApplicationFacade
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
                                                            IConfigurationRoot configurationRoot) =>
        services.AddMediatR(
                    mediatrConfig =>
                    {
                        mediatrConfig.RegisterServicesFromAssembly(typeof(ApplicationFacade).Assembly);
                        mediatrConfig.AddBehavior<VerifyQuotaTableDoesNotExistBehavior>();
                        mediatrConfig.AddOpenBehavior(typeof(ExecutionTimeLoggingBehavior<,>));
                    }
                )
                .AddServiceOptions(configurationRoot);

    private static IServiceCollection AddServiceOptions(this IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        return services.AddOptions()
                       .Configure<BatchingSettings>(configurationRoot.GetSection("BatchSettings"));
    }
}