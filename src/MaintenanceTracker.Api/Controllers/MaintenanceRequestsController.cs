using MaintenanceTracker.Api.Contracts;
using MaintenanceTracker.Api.CurrentUser;
using MaintenanceTracker.Application.MaintenanceRequests;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Controllers;

[ApiController]
[Route("api/maintenance-requests")]
public sealed class MaintenanceRequestsController(ICurrentUserProvider currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MaintenanceRequestDetail>> CreateAsync(
        CreateMaintenanceRequestBody body,
        [FromServices] CreateMaintenanceRequest createMaintenanceRequest,
        CancellationToken cancellationToken)
    {
        var command = new CreateMaintenanceRequestCommand(
            body.Title!, body.Description!, body.Category!.Value, body.Priority!.Value, currentUser.GetUserId());

        var detail = await createMaintenanceRequest.ExecuteAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = detail.Id }, detail);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaintenanceRequestDetail>> GetByIdAsync(
        Guid id,
        [FromServices] GetMaintenanceRequestDetail getDetail,
        CancellationToken cancellationToken) =>
        Ok(await getDetail.ExecuteAsync(id, cancellationToken));

    [HttpPost("{id:guid}/assignments")]
    public async Task<ActionResult<MaintenanceRequestDetail>> AssignAsync(
        Guid id,
        AssignBody body,
        [FromServices] AssignMaintenanceRequest assignMaintenanceRequest,
        CancellationToken cancellationToken)
    {
        var command = new AssignMaintenanceRequestCommand(id, body.AssigneeId!.Value, currentUser.GetUserId());

        return Ok(await assignMaintenanceRequest.ExecuteAsync(command, cancellationToken));
    }

    [HttpPost("{id:guid}/status-changes")]
    public async Task<ActionResult<MaintenanceRequestDetail>> ChangeStatusAsync(
        Guid id,
        ChangeStatusBody body,
        [FromServices] ChangeMaintenanceRequestStatus changeStatus,
        CancellationToken cancellationToken)
    {
        var command = new ChangeMaintenanceRequestStatusCommand(id, body.TargetStatus!.Value, currentUser.GetUserId());

        return Ok(await changeStatus.ExecuteAsync(command, cancellationToken));
    }
}
