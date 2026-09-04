using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Azure.Core;
using TimeKeeper.Domain.Exceptions;
using TimeKeeper.Domain.Security;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Validators;

namespace TimeKeeper.Domain.Models;

public partial class Laborer : ModelBase<Laborer>
{
    #region fields
    #endregion fields

    #region properties

    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PasswordConfirm { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    #endregion properties

    #region ctor
    #endregion ctor

    #region public

    /// <summary>
    /// Returns a flag indicating whther the password and ConfirmPassword meet password requirments 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PasswordValidationException"></exception>
    public bool PasswordIsValid()
    {
        string errorMessage = string.Empty;
        if (!PasswordUtilities.PasswordIsValid(this, out errorMessage))
        {
            throw new PasswordValidationException(errorMessage, this);
        }

        return true;
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
            isDirty = (!isDirty) ? (this.FirstName != this._snapshot.FirstName) : isDirty;
            isDirty = (!isDirty) ? (this.MiddleName != this._snapshot.MiddleName) : isDirty;
            isDirty = (!isDirty) ? (this.LastName  != this._snapshot.LastName) : isDirty;
            isDirty = (!isDirty) ? (this.Email != this._snapshot.Email) : isDirty;
            isDirty = (!isDirty) ? (this.Username != this._snapshot.Username) : isDirty;
        }

        return isDirty;
    }

    public override void RevertToSnapshot()
    {
        throw new NotImplementedException();
    }

    #endregion public
}
