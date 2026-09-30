using MaintenanceTracker.Application.Errors;
using MaintenanceTracker.Application.Users;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class AssignMaintenanceRequest(
    IUserCatalog userCatalog,
    IMaintenanceRequestRepository repository,
    IUnitOfWork unitOfWork,
    GetMaintenanceRequestDetail getDetail,
    TimeProvider timeProvider)
{
    public async Task<MaintenanceRequestDetail> ExecuteAsync(
        AssignMaintenanceRequestCommand command, CancellationToken cancellationToken)
    {
        if (!await userCatalog.ExistsAsync(command.PerformedById, cancellationToken))
        {
            throw UseCaseException.UserNotFound(command.PerformedById);
        }

        var maintenanceRequest = await repository.GetByIdAsync(command.MaintenanceRequestId, cancellationToken)
            ?? throw UseCaseException.MaintenanceRequestNotFound(command.MaintenanceRequestId);

        if (!await userCatalog.ExistsAsync(command.AssigneeId, cancellationToken))
        {
            throw UseCaseException.AssigneeNotFound(command.AssigneeId);
        }

        maintenanceRequest.AssignTo(command.AssigneeId, command.PerformedById, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await getDetail.ExecuteAsync(maintenanceRequest.Id, cancellationToken);
    }
}
