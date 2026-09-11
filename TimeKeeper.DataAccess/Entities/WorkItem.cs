using System;
using System.Collections.Generic;

namespace TimeKeeper.DataAccess.Entities;

public partial class WorkItem
{
    public string? ActivityNumber { get; set; }

    public string? Description { get; set; }

    public int Id { get; set; }

    public bool IsBillable { get; set; }

    public bool IsComplete { get; set; }

    public bool IsDeployed { get; set; }

    public bool IsInReview { get; set; }

    public bool IsOpen { get; set; }

    public string? ProductionVersion { get; set; }

    public virtual Project Project { get; set; } = null!;

    public int ProjectId { get; set; }

    public DateTime? ReviewDate { get; set; }

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    public string Title { get; set; } = null!;

    public int? WorkItemNumber { get; set; }

    public int WorkItemType { get; set; }



    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;
}