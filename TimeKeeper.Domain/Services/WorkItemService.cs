using MediatR;
using System.Linq.Expressions;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Models.DTOs;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.Domain.Services;

/// <summary>
///  Provides application-level operations for creating, retrieving, updating, summarizing, and 
///  deleting work items.
/// </summary>
/// <remarks> 
///  Coordinates work item commands and queries through an <c>IMediator</c> and prevents deletion 
///  when related time entries exist to preserve historical records.</remarks>
public class WorkItemService : IWorkItemService
{
    #region fields

    IMediator? _mediator = null;

    ITimeEntryService? _timeEntryService = null;

    #endregion fields

    #region properties
    #endregion properties

    #region data
    #endregion data

    #region ctor

    public WorkItemService(WorkItemServiceOptions options)
    {
        this._mediator = options.Mediator;
        this._timeEntryService = options.TimeEntrySvc;
    }

    #endregion ctor

    #region private
    #endregion private

    #region public

    /// <summary>
    /// Adds a new WorkItem to the database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<Models.WorkItem?> AddWorkItem(Models.WorkItem model)
    {
        Models.WorkItem? result = (this._mediator != null) ? await this._mediator.Send(new AddWorkItem(model)) : null;
        return result;
    }

    /// <summary>
    ///  Deletes a work item when no time entries are associated with it.
    /// </summary>
    /// <remarks>
    ///  Use archival or closure workflows for work items that contain historical time records.</remarks>
    /// <param name="workitemID">
    ///  The unique identifier of the work item to delete.</param>
    /// <returns>
    ///  The deleted work item if the operation succeeds; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when the work item has one or more associated time entries and cannot be deleted to 
    ///  preserve historical records.</exception>
    public async Task<Models.WorkItem?> DeleteWorkItem(int workitemID)
    {
        int entryCount = await this._timeEntryService!.GetTimeEntryCountForWorkItem(workitemID);
        if(entryCount > 0)
        {
            string errMsg = $"This WorkItem with ID {workitemID} contains {entryCount} time entr" +
                            $"{(entryCount == 1 ? "y" : "ies")} and cannot be deleted." +
                            $"Historical work records must be preserved."+
                            $"Consider archiving or closing the WorkItem instead.";

            throw new InvalidOperationException(errMsg);
        }

        Models.WorkItem? result = (this._mediator != null) ? await this._mediator.Send(new DeleteWorkItem(workitemID)) : null;
        return result;
    }

    /// <summary>
    /// Returns the WorkItem that is associated with the <paramref name="itemID"/> argument.
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public async Task<Models.WorkItem?> GetWorkItemByID(int itemID)
    {
        Models.WorkItem? result = (this._mediator != null) ? await this._mediator.Send(new GetWorkitemById(itemID)) : null;
        return result;
    }

    /// <summary>
    /// Returns a list of DTO objects which contain the Total Minutes worked for each WorkItem 
    /// within specified date range.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public async Task<List<WorkItemDateRangeSummaryDto>> GetWorkItemSummaryByDateRange(DateTime startDate, DateTime endDate)
    {
        List < WorkItemDateRangeSummaryDto> result = (this._mediator == null) ? new List<WorkItemDateRangeSummaryDto>() :
            await this._mediator.Send(new GetWorkItemSummaryForDateRange(startDate, endDate));

        return result;

    }

    /// <summary>
    /// Returns an optionally filtered list of WorkItem objects. 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<List<Models.WorkItem>> GetFilteredWorkItems(List<Expression<Func<DataAccess.Entities.WorkItem, bool>>>? filter = null)
    {
        List<Models.WorkItem> result = (this._mediator != null) ?
                                            await this._mediator.Send(new GetFilteredWorkItems(filter)) :
                                            new List<Models.WorkItem>();
        return result;
    }

    /// <summary>
    /// Updates the IsOpen flag associated with the <paramref name="workitemId"/> argument.
    /// </summary>
    /// <param name="workitemId"></param>
    /// <param name="isOpen"></param>
    /// <returns></returns>
    public async Task<Boolean> ToggleWorkItemStatus(int workitemId, Boolean isOpen)
    {
        bool result = (this._mediator != null) && await this._mediator.Send(new ToggleWorkItemStatus(workitemId, isOpen));
        return result;
    }

    /// <summary>
    /// Updates a WrkItem record from the <paramref name="model"/> argument.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> UpdateWorkItem(Models.WorkItem model)
    {
        bool result = (this._mediator != null) ? await this._mediator.Send(new UpdateWorkItem(model)) : false;
        return result;
    }

    #endregion public
}

/// <summary>
///  Represents configuration dependencies required by a work item service.
/// </summary>
/// <param name="Mediator">
///  Mediator used to dispatch requests and notifications.</param>
/// <param name="TimeEntrySvc">
///  Time entry service used for time entry operations.</param>
public record WorkItemServiceOptions(IMediator Mediator, ITimeEntryService TimeEntrySvc);
