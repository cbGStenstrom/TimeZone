namespace TimeKeeper.Domain.Utilities.Mappers;

public static class WorkItemTypeMapper
{
    public static Models.WorkItemType MapEntityToDomain(this DataAccess.Entities.WorkItemType e) => new()
    {
        Id = e.Id,
        IsNew = false,
        Name = e.Name,
        Description = e.Description,
        Icon = e.Icon,
        IsEnabled = e.IsEnabled,
        CreatedDate = e.CreatedDate,
        CreatedBy = e.CreatedBy,
        UpdatedDate = e.UpdatedDate,
        UpdatedBy = e.UpdatedBy
    };
}
