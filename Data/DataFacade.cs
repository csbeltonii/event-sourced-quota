
using Data.Models;
using Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

public static class DataFacade
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>))
                .AddSingleton(typeof(IEventRepository), typeof(EventRepository));

        return services
               .AddCosmosDb(configurationRoot, [])
               .AddOptions()
               .Configure<CosmosSettings>(configurationRoot.GetSection("CosmosSettings"));
    }
}