using Application.QuotaTables.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace event_sourced_quota.Controllers;

public record GetQuotaTableCellsRequest(string ProjectId, string QuotaTableName, int PageNumber, int PageSize);

[Route("api/quota/cells")]
[ApiController]
public class GetQuotaTableCellsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetQuotaTableCellsAsync([FromQuery] GetQuotaTableCellsRequest request,
                                                             CancellationToken cancellationToken = default)
    {
        var (projectId, quotaTableName, pageNumber, pageSize) = request;

        var cells = await mediator.Send(
            new GetQuotaTableCells(projectId, quotaTableName, pageNumber, pageSize),
            cancellationToken
        );

        return Ok(cells);
    }
}