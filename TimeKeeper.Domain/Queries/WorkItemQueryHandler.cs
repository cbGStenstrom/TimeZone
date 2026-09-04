using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Models.DTOs;
using TimeKeeper.Domain.Utilities;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Queries
{
    public class WorkItemQueryHandler(TimeKeeperDbContext dbCtx) : 
                                        IRequestHandler<GetWorkitemById, Models.WorkItem?>,
                                        IRequestHandler<GetFilteredWorkItems, List<Models.WorkItem>>,
                                        IRequestHandler<GetWorkItemSummaryForDateRange, List<WorkItemDateRangeSummaryDto>>
    {
        #region properties/fields

        readonly TimeKeeperDbContext _dbCtx = dbCtx;

        #endregion properties/fields

        #region ctor
        #endregion ctor

        #region Handlers

        /// <summary>
        /// Returns the single WorkItem associated with the <paramref name="itemID"/> argument.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.WorkItem?> Handle(GetWorkitemById request, CancellationToken cancellationToken)
        {
            WorkItem? entity = await this._dbCtx.WorkItems.Include(e=>e.TimeEntries).FirstOrDefaultAsync(e => e.Id == request.itemID, cancellationToken);
            Models.WorkItem? result = entity?.MapEntityToDomain();

            if (entity?.TimeEntries != null)
            {
                result.TimeEntries = new List<Models.TimeEntry>();
                foreach (DataAccess.Entities.TimeEntry entry in entity.TimeEntries)
                {
                    Models.TimeEntry entryModel = entry.MapEntityToDomain();
                    entryModel.TakeSnapshot();
                    result.TimeEntries.Add(entryModel);
                }
            }

            result?.TakeSnapshot();
            return result;
        }

        /// <summary>
        /// Returns a list of optionally filtered WorkItems,
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<Models.WorkItem>> Handle(GetFilteredWorkItems request, CancellationToken cancellationToken)
        {
            var query = this._dbCtx.WorkItems as IQueryable<WorkItem>;

            if(request.Filter != null)
            {
                foreach(var filter in request.Filter)
                {
                    query = query.Where(filter);
                }
            }

            List<WorkItem> entities = await query.ToListAsync();

            List<Models.WorkItem> results = new List<Models.WorkItem>();
            foreach(WorkItem entity in entities)
            {
                Models.WorkItem model = entity.MapEntityToDomain();

                if(entity.TimeEntries != null)
                {
                    foreach (var timeEntry in entity.TimeEntries)
                    {
                        model.TimeEntries.Add(timeEntry.MapEntityToDomain());
                    }
                }

                model.TakeSnapshot();

                results.Add(model); 
            }

            return results;
        }

        /// <summary>
        /// Returns a list of DTO objects which contain the Total Minutes worked for each WorkItem 
        /// within specified date range.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<List<WorkItemDateRangeSummaryDto>> Handle(GetWorkItemSummaryForDateRange request, CancellationToken cancellationToken)
        {
            DateTime startDate = request.StartDate.ConvertLocalToUtc();
            DateTime endDate   = request.EndDate.ConvertLocalToUtc();

            var query =
                from time in _dbCtx.TimeEntries
                    //where time.StartWork >= startDate && time.EndWork <= endDate
                where (time.StartWork >= startDate && time.StartWork <= endDate) ||
                      (time.EndWork >= startDate && time.EndWork <= endDate)
                join item in _dbCtx.WorkItems
                    on time.WorkItemId equals item.Id
                group new { time, item } by new { item.Id, item.IsOpen, item.Title, item.IsBillable, item.IsInReview } into g
                select new WorkItemDateRangeSummaryDto
                {
                    IsBillable          = g.Key.IsBillable,
                    IsOpen              = g.Key.IsOpen,
                    IsInReview          = g.Key.IsInReview,
                    WorkItemID          = g.Key.Id,
                    WorkItemTitle       = g.Key.Title,
                    TotalMinutesWorked  = g.Sum(x => EF.Functions.DateDiffMinute(x.time.StartWork, x.time.EndWork)) ?? 0
                };

            return query.ToListAsync(cancellationToken);

        }

        #endregion Handlers
    }

    /// <summary>
    /// Returns the single WorkItem associated with the <paramref name="itemID"/> argument.
    /// </summary>
    /// <param name="itemID"></param>
    public record GetWorkitemById(int itemID) : IRequest<Models.WorkItem?>;

    /// <summary>
    /// Returns a list of optionally filtered WorkItems,
    /// </summary>
    /// <param name="Filter"></param>
    public record GetFilteredWorkItems(List<Expression<Func<WorkItem, bool>>>? Filter = null) : IRequest<List<Models.WorkItem>>;

    /// <summary>
    /// Returns a list of DTO objects which contain the Total Minutes worked for each WorkItem 
    /// within specified date range.
    /// </summary>
    /// <param name="StartDate"></param>
    /// <param name="EndDate"></param>
    public record GetWorkItemSummaryForDateRange(DateTime StartDate, DateTime EndDate) : IRequest<List<WorkItemDateRangeSummaryDto>>;

}
