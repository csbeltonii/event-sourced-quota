using Application.QuotaTables.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace event_sourced_quota.Controllers;

public record GetQuotaTableRequest(
    string Id, 
    string ProjectId,
    string QuotaTableName);

[Route("api/quota")]
[ApiController]
public class GetQuotaTableController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetQuotaTableAsync([FromQuery] GetQuotaTableRequest request, CancellationToken cancellationToken = default)
    {
        var (id, projectId, quotaTableName) = request;

        var quotaTable = await mediator.Send(new GetQuotaTable(id, projectId, quotaTableName), cancellationToken);

        return quotaTable is not null
            ? Ok(quotaTable) 
            : NotFound();
    }
}