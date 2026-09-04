using System.Linq.Expressions;
using TimeKeeper.Domain.Models.DTOs;

namespace TimeKeeper.Domain.Services.Interfaces
{
    public interface IWorkItemService
    {
        Task<Models.WorkItem?> AddWorkItem(Models.WorkItem model);
        Task<Models.WorkItem?> DeleteWorkItem(int workitemID);
        Task<List<Models.WorkItem>> GetFilteredWorkItems(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? filter = null);
        Task<Models.WorkItem?> GetWorkItemByID(int itemID);
        Task<List<WorkItemDateRangeSummaryDto>> GetWorkItemSummaryByDateRange(DateTime startDate, DateTime endDate);
        Task<Boolean> ToggleWorkItemStatus(int workitemId, Boolean isOpen);
        Task<bool> UpdateWorkItem(Models.WorkItem model);
    }
}