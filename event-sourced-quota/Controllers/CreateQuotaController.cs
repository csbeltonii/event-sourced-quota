using Application.QuotaTables.Commands;
using Application.QuotaTables.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace event_sourced_quota.Controllers;

public record CreateQuotaTableRequest(
    string projectId,
    string quotaTableName,
    int cellCount);

[Route("api/quota")]
[ApiController]
public class CreateQuotaController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateQuotaTableAsync(CreateQuotaTableRequest request, CancellationToken cancellationToken = default)
    {
        var(projectId, quotaTableName, cellCount) = request;

        try
        {
            var quotaTable = await mediator.Send(
                new CreateQuotaTable(
                    projectId,
                    quotaTableName,
                    cellCount,
                    User.Identity?.Name ?? "event-source-quota"
                ),
                cancellationToken
            );

            return Ok(quotaTable);
        }
        catch (QuotaTableExistsException)
        {
            return Conflict($"{quotaTableName} already exists.");
        }
    }
}