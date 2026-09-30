using MaintenanceTracker.Application.Errors;
using MaintenanceTracker.Application.Users;
using MaintenanceTracker.Domain.MaintenanceRequests;

namespace MaintenanceTracker.Application.MaintenanceRequests;

public sealed class CreateMaintenanceRequest(
    IUserCatalog userCatalog,
    IMaintenanceRequestRepository repository,
    IUnitOfWork unitOfWork,
    GetMaintenanceRequestDetail getDetail,
    TimeProvider timeProvider)
{
    public async Task<MaintenanceRequestDetail> ExecuteAsync(
        CreateMaintenanceRequestCommand command, CancellationToken cancellationToken)
    {
        if (!await userCatalog.ExistsAsync(command.PerformedById, cancellationToken))
        {
            throw UseCaseException.UserNotFound(command.PerformedById);
        }

        var maintenanceRequest = MaintenanceRequest.Create(
            command.Title,
            command.Description,
            command.Category,
            command.Priority,
            command.PerformedById,
            timeProvider.GetUtcNow());

        repository.Add(maintenanceRequest);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await getDetail.ExecuteAsync(maintenanceRequest.Id, cancellationToken);
    }
}
