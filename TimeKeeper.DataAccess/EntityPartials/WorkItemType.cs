namespace TimeKeeper.DataAccess.Entities;
// Handwritten extension; not scaffolding output.
public class WorkItemType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedDate { get; set; }
    public string UpdatedBy { get; set; } = "";
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
