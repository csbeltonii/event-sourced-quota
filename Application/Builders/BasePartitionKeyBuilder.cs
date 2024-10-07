using Microsoft.Azure.Cosmos;

namespace Application.Builders;

public abstract class BasePartitionKeyBuilder
{
    protected PartitionKey GeneratePartitionKey(string projectId, string quotaTableName)
        => new PartitionKeyBuilder()
           .Add(projectId)
           .Add(quotaTableName)
           .Build();
}