using Application.QuotaTables.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace event_sourced_quota.Controllers;

public record SetQuotaCellsRequest(
    string ProjectId,
    string QuotaTableName,
    int CoordinateCount,
    ExecutionMode ExecutionMode);

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
            throw new Exception("Too many cells requested.");
        }

        return Accepted();
    }
}