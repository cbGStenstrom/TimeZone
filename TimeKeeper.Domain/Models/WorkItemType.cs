namespace TimeKeeper.Domain.Models;

public class WorkItemType
{
    public int Id { get; set; }
    public bool IsNew { get; set; } = true;
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int ActivityCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedDate { get; set; }
    public string UpdatedBy { get; set; } = "";
    public WorkItemType Draft() => (WorkItemType)MemberwiseClone();
    public void Validate()
    {
        Name = (Name ?? "").Trim();
        if (Name.Length == 0 || Name.Length > 120) throw new InvalidOperationException("Name is required and must be at most 120 characters.");
        if (Description?.Length > 1000 || Icon?.Length > 120) throw new InvalidOperationException("Description must be at most 1000 characters and Icon at most 120 characters.");
    }
}
