using MaintenanceTracker.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(GetUsers getUsers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummary>>> GetAllAsync(CancellationToken cancellationToken) =>
        Ok(await getUsers.ExecuteAsync(cancellationToken));
}
