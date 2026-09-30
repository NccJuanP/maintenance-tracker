using MaintenanceTracker.Domain.Errors;
using MaintenanceTracker.Domain.MaintenanceRequests;
using Shouldly;

namespace MaintenanceTracker.Domain.Tests.MaintenanceRequests;

public sealed class CreateMaintenanceRequestTests
{
    private const string ValidTitle = "Aire acondicionado sin enfriar";
    private const string ValidDescription = "El equipo de la sala 2 no responde al control.";

    [Fact]
    public void Create_WithValidData_StartsPendingWithoutAssignee()
    {
        var request = MaintenanceRequest.Create(
            ValidTitle, ValidDescription, Category.Equipment, Priority.Critical, TestRequests.Requester, TestRequests.CreatedAt);

        request.Id.ShouldNotBe(Guid.Empty);
        request.Title.ShouldBe(ValidTitle);
        request.Description.ShouldBe(ValidDescription);
        request.Category.ShouldBe(Category.Equipment);
        request.Priority.ShouldBe(Priority.Critical);
        request.Status.ShouldBe(RequestStatus.Pending);
        request.RequesterId.ShouldBe(TestRequests.Requester);
        request.AssigneeId.ShouldBeNull();
        request.CreatedAt.ShouldBe(TestRequests.CreatedAt);
    }

    [Fact]
    public void Create_WithValidData_RecordsCreatedEntryByRequester()
    {
        var request = TestRequests.CreatePending();

        var entry = request.ActivityEntries.ShouldHaveSingleItem().ShouldBeOfType<CreatedActivity>();
        entry.Id.ShouldNotBe(Guid.Empty);
        entry.MaintenanceRequestId.ShouldBe(request.Id);
        entry.PerformedById.ShouldBe(TestRequests.Requester);
        entry.OccurredAt.ShouldBe(TestRequests.CreatedAt);
    }

    [Fact]
    public void Create_TrimsTitleAndDescription()
    {
        var request = MaintenanceRequest.Create(
            $"  {ValidTitle}  ", $"\n {ValidDescription} \t", Category.Software, Priority.Low, TestRequests.Requester, TestRequests.CreatedAt);

        request.Title.ShouldBe(ValidTitle);
        request.Description.ShouldBe(ValidDescription);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void Create_ValidatesTitleLength(int length, bool isValid)
    {
        var title = new string('a', length);

        AssertValidation(() => CreateWith(title: title), "title", isValid);
    }

    [Theory]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void Create_ValidatesDescriptionLength(int length, bool isValid)
    {
        var description = new string('a', length);

        AssertValidation(() => CreateWith(description: description), "description", isValid);
    }

    [Fact]
    public void Create_ValidatesTitleLengthAfterTrimming()
    {
        var exception = Should.Throw<DomainValidationException>(() => CreateWith(title: "   abcd   "));

        exception.Field.ShouldBe("title");
    }

    [Fact]
    public void Create_ValidatesDescriptionLengthAfterTrimming()
    {
        var exception = Should.Throw<DomainValidationException>(() => CreateWith(description: "   abcdefghi   "));

        exception.Field.ShouldBe("description");
    }

    [Fact]
    public void Create_WithUndefinedCategory_Throws()
    {
        var exception = Should.Throw<DomainValidationException>(() => CreateWith(category: (Category)99));

        exception.Field.ShouldBe("category");
        exception.Code.ShouldBe(DomainErrorCodes.ValidationFailed);
    }

    [Fact]
    public void Create_WithUndefinedPriority_Throws()
    {
        var exception = Should.Throw<DomainValidationException>(() => CreateWith(priority: (Priority)99));

        exception.Field.ShouldBe("priority");
        exception.Code.ShouldBe(DomainErrorCodes.ValidationFailed);
    }

    private static MaintenanceRequest CreateWith(
        string title = ValidTitle,
        string description = ValidDescription,
        Category category = Category.Infrastructure,
        Priority priority = Priority.Medium) =>
        MaintenanceRequest.Create(title, description, category, priority, TestRequests.Requester, TestRequests.CreatedAt);

    private static void AssertValidation(Func<MaintenanceRequest> create, string field, bool isValid)
    {
        if (isValid)
        {
            Should.NotThrow(() => create());
            return;
        }

        var exception = Should.Throw<DomainValidationException>(() => create());
        exception.Field.ShouldBe(field);
        exception.Code.ShouldBe(DomainErrorCodes.ValidationFailed);
    }
}
