using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Api.Models
{
    public class WorkItemHoursDTO(WorkItem workItem, double totalHours)
    {
        public int Key { get; set; } = workItem.Id;

        public WorkItem WorkItem { get; set; } = workItem;

        public double TotalHours { get; set; } = totalHours;

    }
}
