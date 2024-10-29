using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Data.Models;
using Domain;
using Domain.Quota;
using Domain.Utility;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.QuotaTables.Commands;

public record QuestionResponse(string QuestionName, IEnumerable<QuestionChoice> Choices);

public record QuestionChoice(string ChoiceName, bool IsSelected, int Value);

public record ChoiceDetails(bool isSelected, int Value);

public record QuestionDetails(Dictionary<string, ChoiceDetails> Choices);

public class QuotaEvaluationContext
{
    public bool IncrementedQuotaTable { get; set; }
}

public record RespondentQualified(string ProjectId, string QuotaTableName) : INotification;

public record SetQualifiedCells(
    string ProjectId, 
    string QuotaTableName, 
    QuestionResponse QuestionResponse) : IRequest<Result>;

// conditions = Q1.R1.isSelected == true

public class SetQualifiedCellsHandler(
    CosmosClient cosmosClient, 
    IOptions<CosmosSettings> cosmosSettings,
    IPublisher publisher,
    ILogger<BaseQuotaCommandHandler<SetQualifiedCells, Result>> logger) 
    : BaseQuotaCommandHandler<SetQualifiedCells, Result>(cosmosClient, cosmosSettings, logger)
{
    protected override async Task<Result> HandleCommandAsync(SetQualifiedCells request, CancellationToken cancellationToken)
    {
        var (projectId, quotaTableName, questionResponse) = request;
        var partitionKey = GeneratePartitionKey(projectId, quotaTableName);
        var quotaEvaluationContext = new QuotaEvaluationContext();
        using var engine = new V8ScriptEngine();
        engine.AddHostType("Console", typeof(Console));       

        var selectedConditions = questionResponse
                                 .Choices
                                 .Select(choice => $"{questionResponse.QuestionName}.Choices.{choice.ChoiceName}.isSelected")
                                 .ToList();

        var valueConditions = questionResponse
                              .Choices
                              .Select(choice => $"{questionResponse.QuestionName}.Choices.{choice.ChoiceName}.value")
                              .ToList();

        var responses = new List<QuestionResponse>
        {
            questionResponse
        };

        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        foreach (var (rootItemName, questionChoices) in responses)
        {
            var questionDetails = new QuestionDetails(
                questionChoices.ToDictionary(
                    choice => choice.ChoiceName, 
                    choice => new ChoiceDetails(choice.IsSelected, choice.Value)));

            var serializedDetails = JsonSerializer.Serialize(
                questionDetails,
                serializerOptions
            );

            engine.Script[rootItemName] = serializedDetails;
            engine.Execute("Console.WriteLine('Got Q1 {0}', Q1)");
            engine.Execute($"{rootItemName} = JSON.parse({rootItemName})");
        }

        var cellQueryIterator = Container.GetItemLinqQueryable<QuotaCell>(
                                             requestOptions: new QueryRequestOptions
                                             {
                                                 PartitionKey = partitionKey
                                             },
                                             linqSerializerOptions: new CosmosLinqSerializerOptions
                                             {
                                                 PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                                             })
                                         .Where(quotaCell => quotaCell.DocumentType == DocumentTypes.QuotaCell)
                                         .ToFeedIterator();

        while (cellQueryIterator.HasMoreResults)
        {
            var response = await cellQueryIterator.ReadNextAsync(cancellationToken);

            foreach (var cell in response.Resource)
            {
                if (!selectedConditions.Any(condition => condition.Contains(cell.Condition)) ||
                    valueConditions.Any(condition => condition.Contains(cell.Condition)))
                {
                    continue;
                }

                var result = (bool)engine.Evaluate(cell.Condition);

                logger.LogInformation(
                    "{ClassName}: Condition {Condition} evaluated to {EvaluationResult}",
                    nameof(SetQualifiedCells),
                    cell.Condition,
                    result
                );

                switch (result)
                {
                    case false:
                        continue;
                    case true:

                        if (quotaEvaluationContext.IncrementedQuotaTable is false)
                        {
                            await publisher.Publish(new RespondentQualified(projectId, quotaTableName), cancellationToken);
                            quotaEvaluationContext.IncrementedQuotaTable = true;
                        }

                        cell.Active++;

                        await Container.UpsertItemAsync(
                            cell,
                            partitionKey,
                            new ItemRequestOptions
                            {
                                IfMatchEtag = cell.Etag
                            },
                            cancellationToken
                        );

                        break;
                }
            }
        }

        return Result.Success();
    }
}