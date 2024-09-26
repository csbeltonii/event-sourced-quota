using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using Data.Models;
using Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

public static class CosmosDbExtensions
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration, IEnumerable<JsonConverter> customConverters)
    {
        var cosmosConnectionString = configuration["CosmosDbConnectionString"];

        var serializerOptions = MainStreetSerializer.Options;

        foreach (var customConverter in customConverters)
        {
            serializerOptions.Converters.Add(customConverter);
        }

        var cosmosOptions = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            Serializer = new CosmosSystemTextJsonSerializer(serializerOptions),
            AllowBulkExecution = true,
        };

        services
            .AddOptions()
            .Configure<CosmosSettings>(configuration.GetSection("CosmosSettings"));

        services.AddSingleton(new CosmosClient(cosmosConnectionString, cosmosOptions));

        return services;
    }

    private class CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions) : CosmosSerializer
    {
        private readonly JsonObjectSerializer systemTextJsonSerializer = new(jsonSerializerOptions);

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (stream.CanSeek
                    && stream.Length == 0)
                {
                    return default;
                }

                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                return (T)systemTextJsonSerializer.Deserialize(stream, typeof(T), default);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            this.systemTextJsonSerializer.Serialize(streamPayload, input, input.GetType(), default);
            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}

