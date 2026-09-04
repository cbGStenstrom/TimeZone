using System;
using System.Collections.Generic;

namespace TimeKeeper.DataAccess.Entities;

public partial class Project
{
    public int Id { get; set; }

    public string LongName { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string? Key { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
