using TimeKeeper.Domain.Models;
namespace TimeKeeper.Domain.Services.Interfaces;

public interface IWorkItemTypeService
{
    Task<List<WorkItemType>> GetTypes(bool enabledOnly = false);
    Task Save(WorkItemType model, Laborer user);
    Task SetEnabled(int id, bool enabled, Laborer user);
    Task Delete(int id);
}
