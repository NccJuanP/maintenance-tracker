using MaintenanceTracker.Api.Contracts;
using MaintenanceTracker.Api.CurrentUser;
using MaintenanceTracker.Application;
using MaintenanceTracker.Application.MaintenanceRequests;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Controllers;

[ApiController]
[Route("api/maintenance-requests")]
public sealed class MaintenanceRequestsController(ICurrentUserProvider currentUser) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MaintenanceRequestDetail>(StatusCodes.Status201Created)]
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

    [HttpGet]
    public async Task<ActionResult<PagedResult<MaintenanceRequestListItem>>> ListAsync(
        [FromQuery] MaintenanceRequestListParameters parameters,
        [FromServices] ListMaintenanceRequests listMaintenanceRequests,
        CancellationToken cancellationToken)
    {
        var query = new MaintenanceRequestListQuery(
            parameters.Page,
            parameters.PageSize,
            parameters.Status,
            parameters.Priority,
            parameters.Category,
            parameters.Search,
            parameters.SortDirection);

        return Ok(await listMaintenanceRequests.ExecuteAsync(query, cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<RequestSummary>> GetSummaryAsync(
        [FromServices] GetRequestSummary getSummary,
        CancellationToken cancellationToken) =>
        Ok(await getSummary.ExecuteAsync(cancellationToken));

    [HttpGet("{id:guid}/activity")]
    public async Task<ActionResult<PagedResult<ActivityEntryDto>>> GetActivityAsync(
        Guid id,
        [FromQuery] PagingParameters paging,
        [FromServices] GetMaintenanceRequestActivity getActivity,
        CancellationToken cancellationToken) =>
        Ok(await getActivity.ExecuteAsync(id, paging.Page, paging.PageSize, cancellationToken));

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
