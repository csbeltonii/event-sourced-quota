using Application.QuotaTables.Commands;
using Application.QuotaTables.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace event_sourced_quota.Controllers;

public record SetQuotaCellsRequest(
    string ProjectId,
    string QuotaTableName,
    int CoordinateCount,
    ExecutionMode ExecutionMode);

public record SetQualifiedCellsRequest(
    string ProjectId,
    string QuotaTableName,
    QuestionResponse QuestionResponse);

[Route("api/quota/qualified-coordinates")]
[ApiController]
public class SetQuotaCellsController(IMediator mediator) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> SetQuotaCellsAsync([FromBody] SetQuotaCellsRequest setQuotaCellsRequest, 
                                                        CancellationToken cancellationToken = default)
    {
        var (projectId, quotaTableName, coordinateCount, executionMode) = setQuotaCellsRequest;

        var coordinates = Enumerable.Range(1, coordinateCount)
                                    .Select(rowNumber => $"E{rowNumber}")
                                    .ToList();

        var result = await mediator.Send(
            new SetCells(projectId, 
                         quotaTableName, 
                         coordinates,
                         executionMode),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new GenericQuotaException("Too many cells requested.");
        }

        return Accepted();
    }

    [HttpPut("evaluations")]
    public async Task<IActionResult> SetQuotaCellsAsync([FromBody] SetQualifiedCellsRequest setQualifiedCellsRequest,
                                                        CancellationToken cancellationToken = default)
    {
        var (projectId, quotaTableName, questionResponse) = setQualifiedCellsRequest;

        var result = await mediator.Send(
            new SetQualifiedCells(
                projectId,
                quotaTableName,
                questionResponse
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new GenericQuotaException();
        }

        return Accepted();
    }
}