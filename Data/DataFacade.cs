
using System.Text.Json.Serialization;
using Data.Models;
using Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

public static class DataFacade
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfigurationRoot configurationRoot, IEnumerable<JsonConverter> converters)
    {
        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>))
                .AddSingleton(typeof(IEventRepository), typeof(EventRepository));

        return services
               .AddCosmosDb(configurationRoot, converters)
               .AddOptions()
               .Configure<CosmosSettings>(configurationRoot.GetSection("CosmosSettings"));
    }
}