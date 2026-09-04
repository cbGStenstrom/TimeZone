using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Commands
{
    public  class WorkItemCommandHandler(TimeKeeperDbContext ctx) :
                IRequestHandler<AddWorkItem, Models.WorkItem>,
                IRequestHandler<DeleteWorkItem, Models.WorkItem?>,
                IRequestHandler<UpdateWorkItem, Boolean>,
                IRequestHandler<ToggleWorkItemStatus, Boolean>
    {
        #region properties/fields

        TimeKeeperDbContext _dbCtx = ctx;

        #endregion properties/fields

        #region Handlers

        /// <summary>
        /// Adds a new WorkItem and any navigation properties that may be associated with it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.WorkItem> Handle(AddWorkItem request, CancellationToken cancellationToken)
        {
            WorkItem entity = request.Model.MapDomainToEntity();
            entity.CreatedDate = entity.UpdatedDate = DateTime.UtcNow;

            this._dbCtx.WorkItems.Add(entity);
            await this._dbCtx.SaveChangesAsync(cancellationToken);

            Models.WorkItem result = entity.MapEntityToDomain();
            result.TakeSnapshot();
            return result;
        }

        /// <summary>
        /// Permanently Deletes the existing WorkItem which is associated with the <paramref name="WorkItemID"/> 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.WorkItem?> Handle(DeleteWorkItem request, CancellationToken cancellationToken)
        {
            WorkItem? entity = await this._dbCtx.WorkItems.FirstOrDefaultAsync(e=>e.Id == request.WorkItemID);

            if (entity == null) { return null; }

            this._dbCtx.WorkItems.Remove(entity);
            await this._dbCtx.SaveChangesAsync(cancellationToken);

            Models.WorkItem result = entity.MapEntityToDomain();
            return result;
        }

        /// <summary>
        ///  Toggles the Open/Closed status of the WorkItemID 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Handle(ToggleWorkItemStatus request, CancellationToken cancellationToken)
        {
            WorkItem? existingEntity = await this._dbCtx.WorkItems.FirstOrDefaultAsync(e => e.Id == request.WorkItemID, cancellationToken);

            if (existingEntity == null) return false;

            existingEntity.IsOpen = request.IsOpen;
            return (await this._dbCtx.SaveChangesAsync(cancellationToken)) > 0;
        }

        /// <summary>
        /// Updates the <paramref name="Model"/> argument and any navigation properties that may be 
        /// populated.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Boolean> Handle(UpdateWorkItem request, CancellationToken cancellationToken)
        {
            bool result = false;

            WorkItem? existingEntity = await this._dbCtx.WorkItems.Include(e => e.TimeEntries).FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);

            if (existingEntity != null)
            {
                request.Model.MapDomainToEntity(ref existingEntity);
                await this._dbCtx.SaveChangesAsync(cancellationToken);
                request.Model.TakeSnapshot();
                result = true;
            }

            return result;

        }

        #endregion Handlers

    }

    /// <summary>
    /// Adds a new WorkItem and any navigation properties that may be associated with it.
    /// </summary>
    /// <param name="Model"></param>
    public record AddWorkItem(Models.WorkItem Model) : IRequest<Models.WorkItem>;

    /// <summary>
    /// Permanently Deletes the existing WorkItem which is associated with the <paramref name="WorkItemID"/> 
    /// argument.
    /// </summary>
    /// <param name="WorkItemID"></param>
    public record DeleteWorkItem(int WorkItemID) : IRequest<Models.WorkItem?>;

    /// <summary>
    /// Updates the IsOpen flag associated with the <paramref name="WorkItemID"/> argument.
    /// </summary>
    /// <param name="WorkItemID"></param>
    /// <param name="IsOpen"></param>
    public record ToggleWorkItemStatus(int WorkItemID, bool IsOpen) : IRequest<bool>;

    /// <summary>
    /// Updates the <paramref name="Model"/> argument and any navigation properties that may be 
    /// populated.
    /// </summary>
    /// <param name="Model"></param>
    public record UpdateWorkItem(Models.WorkItem Model) : IRequest<Boolean>;

}
