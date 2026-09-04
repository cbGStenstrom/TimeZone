using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Models;

public partial class TimeEntry : ModelBase<TimeEntry>
{
    #region properties 

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

    public virtual WorkItem? WorkItem { get; set; } = null!;

    public double? HoursWorked { get { return this.CalculateHoursWorked(); } }

    #endregion properties

    #region private

    /// <summary>
    /// Calculates the hours worked.
    /// </summary>
    /// <returns></returns>
    double? CalculateHoursWorked()
    {
        double? result = 0;

        if(this.EndWork.HasValue && this.StartWork.HasValue)
        {
            TimeSpan interval = this.EndWork.Value - this.StartWork.Value;
            int totalMinutes = (int)interval.TotalMinutes;

            int hours = (int)(totalMinutes / 60);
            int minutes = totalMinutes - (60 * hours);

            double quarterHour = 0;

            if(minutes > 8 && minutes < 22)
            {
                quarterHour = .25;
            }
            else if (minutes > 21 && minutes < 37)
            {
                quarterHour = .5;
            }
            else if (minutes > 36 && minutes < 51)
            {
                quarterHour = .75;
            }
            else if (minutes > 50 )
            {
                hours++;
            }

            result = hours + quarterHour;

        }

        return result;
    }

    #endregion private

    #region public

    /// <summary>
    /// Returns the currently saved copy of this object
    /// </summary>
    /// <returns></returns>
    public TimeEntry? GetSnapshot()
    {
        return base._snapshot;
    }

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
            isDirty = (!isDirty) ? (this.StartWork != this._snapshot.StartWork) : isDirty;
            isDirty = (!isDirty) ? (this.EndWork != this._snapshot.EndWork) : isDirty;
            isDirty = (!isDirty) ? (this.Accomplishment != this._snapshot.Accomplishment) : isDirty;
            isDirty = (!isDirty) ? (this.LaborerId != this._snapshot.LaborerId) : isDirty;
            isDirty = (!isDirty) ? (this.WorkItemId != this._snapshot.WorkItemId) : isDirty;
        }

        return isDirty;
    }

    /// <summary>
    ///  Determines whether the current object is in a valid state for processing.
    /// </summary>
    /// <returns>
    ///  true if the object has a positive WorkItemId and a value assigned to StartWork; otherwise, false.
    /// </returns>
    public Boolean IsValid()
    {
        bool isValid = true;
            
        isValid = (isValid) ? this.WorkItemId > 0 : isValid;
        isValid = (isValid) ? this.StartWork.HasValue : isValid;

        return isValid;
    }

    /// <summary>
    /// Replaces the current object values with those of the snapshot
    /// </summary>
    public override void RevertToSnapshot()
    {
        if (base._snapshot != null)
        {
            this.CreatedBy = base._snapshot.CreatedBy;
            this.CreatedDate = base._snapshot.CreatedDate;
            this.UpdatedDate = base._snapshot.UpdatedDate;
            this.UpdatedBy = base._snapshot.UpdatedBy;
            this.Id = base._snapshot.Id;
            this.WorkItemId = base._snapshot.WorkItemId;
            this.Accomplishment = base._snapshot.Accomplishment;
            this.EndWork= base._snapshot.EndWork;
            this.StartWork = base._snapshot.StartWork;
            this.LaborerId = base._snapshot.LaborerId;
        }
    }

    /// <summary>
    /// Takes a "SnapShot" of the state of the object at the time it is called. This snapshot 
    /// is available using the "_snapshot" property
    /// </summary>
    public override void TakeSnapshot()
    {
        base.TakeSnapshot();

        if (this.Laborer != null)
        {
            this.Laborer.TakeSnapshot();
        }

        if (this.WorkItem != null)
        {
            this.WorkItem.TakeSnapshot();
        }
    }

    #endregion public

}
