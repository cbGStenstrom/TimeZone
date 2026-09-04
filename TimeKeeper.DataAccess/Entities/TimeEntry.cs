using System;
using System.Collections.Generic;

namespace TimeKeeper.DataAccess.Entities;

public partial class TimeEntry
{
    public int Id { get; set; }

    public int LaborerId { get; set; }

    public int WorkItemId { get; set; }

    public DateTime? StartWork { get; set; }

    public DateTime? EndWork { get; set; }

    public string? Accomplishment { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual Laborer Laborer { get; set; } = null!;

    public virtual WorkItem WorkItem { get; set; } = null!;
}
