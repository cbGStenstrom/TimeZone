namespace TimeKeeper.DataAccess.Entities;

public partial class WorkItem
{
    public virtual WorkItemType Type { get; set; } = null!;
}
