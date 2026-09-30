using MaintenanceTracker.Application.Errors;
using MaintenanceTracker.Application.Users;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class ChangeMaintenanceRequestStatus(
    IUserCatalog userCatalog,
    IMaintenanceRequestRepository repository,
    IUnitOfWork unitOfWork,
    GetMaintenanceRequestDetail getDetail,
    TimeProvider timeProvider)
{
    public async Task<MaintenanceRequestDetail> ExecuteAsync(
        ChangeMaintenanceRequestStatusCommand command, CancellationToken cancellationToken)
    {
        if (!await userCatalog.ExistsAsync(command.PerformedById, cancellationToken))
        {
            throw UseCaseException.UserNotFound(command.PerformedById);
        }

        var maintenanceRequest = await repository.GetByIdAsync(command.MaintenanceRequestId, cancellationToken)
            ?? throw UseCaseException.MaintenanceRequestNotFound(command.MaintenanceRequestId);

        maintenanceRequest.ChangeStatus(command.TargetStatus, command.PerformedById, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await getDetail.ExecuteAsync(maintenanceRequest.Id, cancellationToken);
    }
}
