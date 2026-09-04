using System;
using System.Collections.Generic;

namespace TimeKeeper.Domain.Models;

public partial class Project : ModelBase<Project>
{
    #region Properties 

    public int Id { get; set; }

    public string LongName { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string? Key { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();

    #endregion Properties

    #region public

    /// <summary>
    /// Returns a flag indicating whether the object has been modified since the last "snapshot" 
    /// or object creation.
    /// </summary>
    /// <returns></returns>
    public Boolean IsDirty()
    {
        bool isDirty = false;

        // If the _snapshot is not yet set then this would be a NEW model and therefore, by definition
        // is intrisically "dirty"
        //
        if (this._snapshot != null)
        {
            isDirty = (!isDirty) ? (this.LongName != this._snapshot.LongName) : isDirty;
            isDirty = (!isDirty) ? (this.ShortName != this._snapshot.ShortName) : isDirty;
            isDirty = (!isDirty) ? (this.Key != this._snapshot.Key) : isDirty;
        }

        return isDirty;
    }

    /// <summary>
    ///  Restores the object's state to match the most recently saved snapshot.
    /// </summary>
    /// <remarks>
    ///  If no snapshot has been saved, this method has no effect. Use this method to undo changes 
    ///  and revert the object's properties to their previous values as captured by the snapshot.
    /// </remarks>
    public override void RevertToSnapshot()
    {
        if (this._snapshot != null)
        {
            this.LongName   = this._snapshot.LongName;
            this.ShortName  = this._snapshot.ShortName;
            this.Key        = this._snapshot.Key;
        }
    }

    /// <summary>
    /// Takes a "SnapShot" of the state of the object at the time it is called. This snapshot 
    /// is available using the "_snapshot" property
    /// </summary>
    public override void TakeSnapshot()
    {
        base.TakeSnapshot();

        foreach (var item in WorkItems)
        {
            item.TakeSnapshot();
        }
    }

    #endregion public

}
