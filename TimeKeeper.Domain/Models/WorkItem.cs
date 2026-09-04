using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TimeKeeper.Domain.Enums;

namespace TimeKeeper.Domain.Models;

public partial class WorkItem : ModelBase<WorkItem>
{
    #region properties

    public bool IsBillable { get; set; }

    public int Id { get; set; }

    public bool IsNew { get { return (this.Id == default(int)); } }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public bool IsInReview { get; set; }

    public bool IsOpen { get; set; }

    public WorkItemType? WorkItemType { get; set; }



    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    #endregion properties

    #region public

    /// <summary>
    /// Returns a flag indicating whether the object has been modified since the last "snapshot" 
    /// or object creation.
    /// </summary>
    /// <returns></returns>
    public Boolean IsDirty()
    {
        // If this instance repesents a new, unsaved object, it is by definition "dirty"
        //
        bool isDirty = this.IsNew;

        // If the _snapshot is not yet set then this would be a NEW model and therefore, by definition
        // is intrisically "dirty"
        //
        if (this._snapshot != null)
        {
            isDirty = (!isDirty) ? (this.IsOpen != this._snapshot.IsOpen) : isDirty;
            isDirty = (!isDirty) ? (this.Title != this._snapshot.Title) : isDirty;
            isDirty = (!isDirty) ? (this.Description != this._snapshot.Description) : isDirty;
            isDirty = (!isDirty) ? (this.ProjectId != this._snapshot.ProjectId) : isDirty;
            isDirty = (!isDirty) ? (this.IsBillable != this._snapshot.IsBillable) : isDirty;
            isDirty = (!isDirty) ? (this.IsInReview != this._snapshot.IsInReview) : isDirty;
            isDirty = (!isDirty) ? (this.WorkItemType != this._snapshot.WorkItemType) : isDirty;
        }
        else
        {
            isDirty = true;
        }

        // If any TimeEntries are defined then examine them for changes as well
        //
        if (!isDirty && this.TimeEntries != null)
        {
            foreach (TimeEntry item in this.TimeEntries)
            {
                isDirty = (!isDirty) ? item.IsDirty() : isDirty;
            }
        }

        return isDirty;
    }

    /// <summary>
    /// Replaces the current object values with those of the snapshot
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public override void RevertToSnapshot()
    {
        if (base._snapshot != null)
        {
            this.CreatedBy = base._snapshot.CreatedBy;
            this.CreatedDate = base._snapshot.CreatedDate;
            this.UpdatedDate = base._snapshot.UpdatedDate;
            this.UpdatedBy = base._snapshot.UpdatedBy;
            this.Id = base._snapshot.Id;
            this.IsInReview = base._snapshot.IsInReview;
            this.ProjectId = base._snapshot.ProjectId;
            this.Title = base._snapshot.Title;
            this.IsOpen = base._snapshot.IsOpen;
            this.Description = base._snapshot.Description;
            this.IsBillable = base._snapshot.IsBillable;
            this.WorkItemType = base._snapshot.WorkItemType;
        }

        // If any TimeEntries are defined then revert any changes made to them as well.
        //
        if (this.TimeEntries != null)
        {
            foreach (TimeEntry item in this.TimeEntries)
            {
                item.RevertToSnapshot();
            }
        }
    }

    /// <summary>
    /// Takes a "SnapShot" of the state of the object at the time it is called. This snapshot 
    /// is available using the "_snapshot" property
    /// </summary>
    public override void TakeSnapshot()
    {
        base.TakeSnapshot();

        if (base._snapshot != null)
        {
            base._snapshot.TimeEntries = new List<TimeEntry>();

            foreach (var timeEntry in this.TimeEntries)
            {
                TimeEntry? item = timeEntry.GetSnapshot();

                if (item != null)
                {
                    base._snapshot.TimeEntries.Add(item);
                }
            }
        }
    }

    #endregion public

}