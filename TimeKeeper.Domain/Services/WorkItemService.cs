using System.Collections.Generic;
using System.Linq.Expressions;
using MediatR;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Models.DTOs;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Services;

public class WorkItemService : IWorkItemService
{
    #region fields

    IMediator? _mediator = null;

    #endregion fields

    #region properties
    #endregion properties

    #region data
    #endregion data

    #region ctor

    public WorkItemService(WorkItemServiceOptions options)
    {
        this._mediator = options.Mediator;
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
    /// Permanently Deletes the workitem associated with the <paramref name="workitemID"/> 
    /// argument.
    /// </summary>
    /// <param name="workitemID"></param>
    /// <returns></returns>
    public async Task<Models.WorkItem?> DeleteWorkItem(int workitemID)
    {
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

public record WorkItemServiceOptions(IMediator Mediator);
