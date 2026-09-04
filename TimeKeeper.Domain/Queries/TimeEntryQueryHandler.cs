using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Queries;

/// <summary>
///  Handles queries related to time entry retrieval and filtering within the timekeeping system.
/// </summary>
/// <remarks>
///  This handler processes requests for retrieving filtered lists of time entries, fetching a 
///  specific time entry by its identifier, and obtaining the currently active time entry for a work 
///  item. It is intended to be used with MediatR request/response patterns. All operations are 
///  performed asynchronously and rely on the provided database context for data access.
/// </remarks>
/// <param name="dbCtx">
///  The database context used to access time entry and related data. Cannot be null.
/// </param>
public class TimeEntryQueryHandler(TimeKeeperDbContext dbCtx) : 
                                    IRequestHandler<GetFilteredTimeEntries, IEnumerable<Models.TimeEntry>>,
                                    IRequestHandler<GetTimeEntryById, Models.TimeEntry>,
                                    IRequestHandler<GetActiveTimeEntry, Models.TimeEntry?>
{
    #region properties

    private readonly TimeKeeperDbContext _dbCtx = dbCtx;

    #endregion properties

    #region data
    #endregion data

    #region ctor
    #endregion ctor

    #region private
    #endregion private

    #region public

    /// <summary>
    /// Returns a filtered list of TimeEntry records
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.TimeEntry>> Handle(GetFilteredTimeEntries request, CancellationToken cancellationToken = default)
    {
        var query = this._dbCtx.TimeEntries.Include(entry => entry.WorkItem) as IQueryable<DataAccess.Entities.TimeEntry>;

        if (request.Filter != null)
        {
            foreach (var filter in request.Filter)
            {
                query = query.Where(filter);
            }
        }

        List<DataAccess.Entities.TimeEntry> entities = await query.ToListAsync();
        List<Models.TimeEntry> result = new List<Models.TimeEntry>();

        if(entities != null)
        {
            foreach(var entity in entities)
            {
                var model = entity.MapEntityToDomain();
                result.Add(model);
            }
        }

        return result;
    }

    /// <summary>
    ///  Retrieves a time entry by its unique identifier.
    /// </summary>
    /// <param name="request">
    ///  The request containing the identifier of the time entry to retrieve. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    ///  A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains the time 
    ///  entry that matches the specified identifier, or a new instance if no such entry exists.
    /// </returns>
    public async Task<Models.TimeEntry> Handle(GetTimeEntryById request, CancellationToken cancellationToken = default)
    {
        var query = this._dbCtx.TimeEntries.Include(entry => entry.WorkItem).Where(e=>e.Id == request.TimeEntryId) as IQueryable<DataAccess.Entities.TimeEntry>;

        var dd = query.ToList();
        DataAccess.Entities.TimeEntry? entity = await query.FirstOrDefaultAsync(cancellationToken);
        Models.TimeEntry result = entity?.MapEntityToDomain() ?? new Models.TimeEntry();

        return result;
    }

    /// <summary>
    ///  Retrieves the active time entry associated with the specified work item.
    /// </summary>
    /// <param name="request">
    ///  The request containing information about the work item for which to retrieve the active 
    ///  time entry.
    /// </param>
    /// <param name="cancellationToken">
    ///  A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    ///  A <see cref="Models.TimeEntry"/> representing the active time entry for the specified 
    ///  work item. Returns a default <see cref="Models.TimeEntry"/> if no active entry is found.
    /// </returns>
    public async Task<Models.TimeEntry?> Handle(GetActiveTimeEntry request, CancellationToken cancellationToken = default)
    {
        var query = this._dbCtx.TimeEntries.Include(entry => entry.WorkItem)
                                            .Where(e => !e.EndWork.HasValue ) as IQueryable<DataAccess.Entities.TimeEntry>;

        DataAccess.Entities.TimeEntry? entity = await query.FirstOrDefaultAsync(cancellationToken);
        Models.TimeEntry? result = entity?.MapEntityToDomain();

        return result;
    }

    #endregion public

}

public record GetActiveTimeEntry() : IRequest<Models.TimeEntry?>;

/// <summary>
/// Returns a filtered list of TimeEntry records
/// </summary>
/// <param name="Filter"></param>
public record GetFilteredTimeEntries(List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>? Filter = null) : IRequest<IEnumerable<Models.TimeEntry>>;

/// <summary>
///  Represents a request to retrieve a time entry by its unique identifier.
/// </summary>
/// <param name="TimeEntryId">
///  The unique identifier of the time entry to retrieve. Must be a positive integer.
/// </param>
public record GetTimeEntryById(int TimeEntryId) : IRequest<Models.TimeEntry>;

