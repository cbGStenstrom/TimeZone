using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TimeKeeper.DataAccess.Entities;
namespace TimeKeeper.Domain.Commands;

#region handler

/// <summary>
///  Handles commands that create, update, enable or disable, and delete work item types using the 
///  data context.
/// </summary>
/// <remarks>
///  Enforces unique work item type names, updates audit fields on changes, and prevents deletion 
///  when related work items exist.</remarks>
/// <param name="ctx">
///  Database context used to query and persist work item type data.</param>
public class WorkItemTypeCommandHandler(TimeKeeperDbContext ctx) :
    IRequestHandler<SaveWorkItemType, bool>, 
    IRequestHandler<SetWorkItemTypeEnabled, bool>, 
    IRequestHandler<DeleteWorkItemType, bool> 
{

    #region public methods

    /// <summary>
    ///  Deletes the specified work item type when it exists and is not referenced by any work items.
    /// </summary>
    /// <param name="request">
    ///  The delete request containing the identifier of the work item type.</param>
    /// <param name="cToken">
    ///  A token to monitor for cancellation of the operation.</param>
    /// <returns>
    ///  <see langword="true"/> when the work item type is deleted; <see langword="false"/> when no 
    ///  matching work item type is found.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when one or more work items reference the work item type.</exception>
    public async Task<bool> Handle(DeleteWorkItemType request, CancellationToken cToken)
    {
        var entity = await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == request.Id, cToken);
        
        if (entity == null) return false;

        if (await ctx.WorkItems.AnyAsync(w => w.WorkItemType == request.Id, cToken))
        {
            throw new InvalidOperationException("Activities now reference this type. Refresh and confirm disabling it instead.");
        }

        ctx.WorkItemTypes.Remove(entity); 
        await Save(cToken); 
        return true;
    }

    /// <summary>
    ///  Creates a new work item type or updates an existing one, then persists the changes.
    /// </summary>
    /// <param name="request">
    ///  The save request containing the work item type model and user information.</param>
    /// <param name="cToken">
    ///  A token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///  <see langword="true"/> when changes are saved; <see langword="false"/> when no values changed 
    ///  for an existing work item type.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when another work item type already uses the same name, or when the target work item 
    ///  type no longer exists.</exception>
    public async Task<bool> Handle(SaveWorkItemType request, CancellationToken cToken)
    {
        var m = request.Model; m.Validate();
        if (await ctx.WorkItemTypes.AnyAsync(t => (m.IsNew || t.Id != m.Id) && t.Name == m.Name, cToken))
        {
            throw new InvalidOperationException("An Activity Type with this name already exists.");
        }

        // WHAT: Create a new entity if the model is new; otherwise, retrieve the existing entity
        //  by its identifier. If the entity does not exist, throw an exception indicating that the
        //  activity type no longer exists and suggest refreshing the list.
        var entity = m.IsNew ? new WorkItemType() : 
                               await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == m.Id, cToken) ?? 
                               throw new InvalidOperationException("Activity Type no longer exists. Refresh the list.");

        // WHAT: If the model is not new and all properties match the existing entity, return false to
        //  indicate that no changes were made.
        if (!m.IsNew && entity.Name == m.Name && entity.Description == m.Description && entity.Icon == m.Icon && entity.IsEnabled == m.IsEnabled)
        {
            return false;
        }

        // WHAT: If the model is new, set the created date and created by properties, and add the entity
        //  to the context. Then, update the entity's properties with the values from the model, set
        //  the updated date and updated by properties, save the changes, and update the model's Id and
        //  IsNew properties. Finally, return true to indicate that changes were made.
        if (m.IsNew) 
        { 
            entity.CreatedDate = DateTime.UtcNow; 
            entity.CreatedBy = request.User.Username; 
            ctx.WorkItemTypes.Add(entity); 
        }

        // WHAT: Update the entity's properties with the values from the model, set the updated date
        //  and updated by properties.
        entity.Name = m.Name; 
        entity.Description = m.Description; 
        entity.Icon = m.Icon; 
        entity.IsEnabled = m.IsEnabled;
        entity.UpdatedDate = DateTime.UtcNow; 
        entity.UpdatedBy = request.User.Username;

        // WHAT: Save the changes to the database and update the model's Id and IsNew properties. 
        await Save(cToken); 
        m.Id = entity.Id; 
        m.IsNew = false;

        // WHAT: Return true to indicate that changes were made and the save operation was successful.
        return true;
    }

    /// <summary>
    ///  Updates the enabled state of a work item type and persists the change.
    /// </summary>
    /// <remarks>
    ///  When a change is applied, updates audit fields for last update time and user before
    ///  saving.</remarks>
    /// <param name="request">
    ///  Request containing the work item type identifier, target enabled state, and user information.
    /// </param>
    /// <param name="cToken">
    ///  Token used to cancel the asynchronous operation.</param>
    /// <returns>
    ///  <see langword="true"/> when the enabled state is changed and saved; otherwise, <see langword="false"/> 
    ///  when the requested state matches the current state.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when the work item type no longer exists.</exception>
    public async Task<bool> Handle(SetWorkItemTypeEnabled request, CancellationToken cToken)
    {
        // WHAT: Retrieve the work item type entity by its identifier. If it does not exist, throw
        //  an exception indicating that the activity type no longer exists and suggest refreshing
        //  the list.
        var entity = await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == request.Id, cToken) ?? 
                            throw new InvalidOperationException("Activity Type no longer exists. Refresh the list.");

        // WHAT: If the current enabled state matches the requested state, return false to indicate
        //  that no changes were made. 
        if (entity.IsEnabled == request.Enabled) return false;

        // WHAT: Update the entity's enabled state and audit fields for last update time and user.
        entity.IsEnabled = request.Enabled; 
        entity.UpdatedDate = DateTime.UtcNow; 
        entity.UpdatedBy = request.User.Username;

        // WHAT: Save the changes to the database and return true to indicate that the enabled state
        // was changed and the save operation was successful.
        await Save(cToken); 
        return true;
    }

    #endregion public methods

    #region helper methods

    /// <summary>
    ///  Persists pending changes to the data store asynchronously.
    /// </summary>
    /// <remarks>
    ///  Clears the change tracker when the save fails or is canceled so failed state is not reused 
    ///  by later operations.</remarks>
    /// <param name="ct">
    ///  A token used to propagate notification that the operation should be canceled.</param>
    /// <returns>
    ///  A task that represents the asynchronous save operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when a database constraint violation occurs, such as a duplicate activity type name 
    ///  or a reference that prevents the type from being updated as requested.</exception>
    async Task Save(CancellationToken ct)
    {
        try 
        { 
            await ctx.SaveChangesAsync(ct); 
        }

        // WHAT: Catch database update exceptions that are caused by SQL constraint violations, such as
        //  duplicate names or foreign key references. Clear the change tracker to prevent failed state
        //  from being reused by later operations, and throw an InvalidOperationException with a
        //  user-friendly message indicating the specific constraint violation.
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627 || sql.Number == 547))
        {
            ctx.ChangeTracker.Clear();
            throw new InvalidOperationException(sql.Number == 547 ? "Activities reference this type. Refresh and confirm disabling it instead." : "An Activity Type with this name already exists.", ex);
        }
        catch
        {
            // WHAT: Clear the change tracker to prevent failed state from being reused by later
            //  operations, and rethrow the exception to propagate it to the caller.
            ctx.ChangeTracker.Clear();
            throw;
        }
    }

    #endregion helper methods
}

#endregion handler

/// <summary>
///  Represents a request to delete a work item by its identifier.
/// </summary>
/// <param name="Id">
///  The identifier of the work item to delete.</param>
public record DeleteWorkItemType(int Id) : IRequest<bool>;

/// <summary>
///  Represents a request to save a work item type for a specific user.
/// </summary>
/// <remarks>
///  Handled as a MediatR request that returns <see langword="true"/> when the save operation succeeds;
///  otherwise <see langword="false"/>.</remarks>
/// <param name="Model">
///  Work item type data to save.</param>
/// <param name="User">
///  User performing the save operation.</param>
public record SaveWorkItemType(Models.WorkItemType Model, Models.Laborer User) : IRequest<bool>;

/// <summary>
///  Represents a request to set whether a work item type is enabled for a specified user.
/// </summary>
/// <remarks>
///  Implements <c>IRequest&lt;bool&gt;</c>.</remarks>
/// <param name="Id">
///  The identifier of the work item type.</param>
/// <param name="Enabled">
///  A value indicating whether the work item type is enabled.</param>
/// <param name="User">
///  The user associated with the change.</param>
public record SetWorkItemTypeEnabled(int Id, bool Enabled, Models.Laborer User) : IRequest<bool>;
