using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Utilities;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Commands
{
    public class TimeEntryCommandHandler(TimeKeeperDbContext ctx, TimeEntryServiceOptions options) :
            IRequestHandler<CreateTimeEntry, Models.TimeEntry?>,
            IRequestHandler<EndWorkOnWorkItem, Models.TimeEntry?>,
            IRequestHandler<StartWorkOnWorkItem, Models.TimeEntry>,
            IRequestHandler<UpdateTimeEntry, Models.TimeEntry>,
            IRequestHandler<DeleteTimeEntry, bool>
    {
        #region properties

        TimeKeeperDbContext _dbCtx = ctx;

        IMediator? _mediator = options.Mediator;

        #endregion properties

        #region data
        #endregion data

        #region ctor
        #endregion ctor

        #region public

        public async Task<Models.TimeEntry?> Handle(CreateTimeEntry request, CancellationToken cancellationToken)
        {
            Models.TimeEntry? result = null;
            TimeEntry? entity = request.Model.MapDomainToEntity();

            entity.CreatedBy    = entity.UpdatedBy   = request.User.Username;
            entity.CreatedDate  = entity.UpdatedDate = DateTime.UtcNow;
            entity.LaborerId    = request.User.Id;

            await this._dbCtx.TimeEntries.AddAsync(entity, cancellationToken);
            await this._dbCtx.SaveChangesAsync(cancellationToken);

            result = entity.MapEntityToDomain();

            return result;
        }

        public async Task<bool> Handle(DeleteTimeEntry request, CancellationToken cancellationToken)
        {
            bool result = false;

            DataAccess.Entities.TimeEntry? entity = await this._dbCtx.TimeEntries.FirstOrDefaultAsync(e => e.Id == request.Model.Id);

            if (entity != null)
            {
                this._dbCtx.TimeEntries.Remove(entity);
                result = this._dbCtx.SaveChanges() > 0;
            }

            return result;
        }

        public async Task<Models.TimeEntry?> Handle(EndWorkOnWorkItem request, CancellationToken cancellationToken)
        {
            Models.TimeEntry? result = null;

            DataAccess.Entities.TimeEntry? entity = await this._dbCtx.TimeEntries.FirstOrDefaultAsync(e => e.Id == request.Model.Id);

            if(entity != null)
            {
                entity.EndWork          = DateTime.UtcNow;
                entity.UpdatedDate      = DateTime.UtcNow;
                entity.UpdatedBy        = request.Username;
                entity.Accomplishment   = request.Model.Accomplishment;

                WorkItem? workItem = await this._dbCtx.WorkItems.FirstOrDefaultAsync(e => e.Id == entity.WorkItemId);

                if (workItem != null)
                {
                    workItem.IsOpen     = !request.CloseWorkItem;
                    workItem.IsInReview = request.SubmittedForReview;
                }

                await this._dbCtx.SaveChangesAsync();
                result = entity.MapEntityToDomain();
            }

            return result;
        }

        public async Task<Models.TimeEntry> Handle(StartWorkOnWorkItem request, CancellationToken cancellationToken)
        {
            // We have to make sure that there is not already an active TimeENtry
            //
            Models.TimeEntry? activeTimeEntry = (this._mediator != null) ?
                await this._mediator.Send(new GetActiveTimeEntry()) : null;

            if (activeTimeEntry != null)
            {
                var msg = $"There is already an active TimeEntry for {activeTimeEntry.WorkItem?.Title}. Close this entry before beginning another.";
                throw new SystemException(msg);
            }

            // If there is not already an active TimeEntry then begin the new one.
            //
            Models.TimeEntry? result = null;
            TimeEntry? entity = request.Model.MapDomainToEntity();

            entity.CreatedBy    = entity.UpdatedBy = request.User.Username;
            entity.CreatedDate  = entity.UpdatedDate = DateTime.UtcNow;
            entity.StartWork    = DateTime.UtcNow;
            entity.LaborerId    = request.User.Id;

            await this._dbCtx.TimeEntries.AddAsync(entity);
            await this._dbCtx.SaveChangesAsync();


            result = entity.MapEntityToDomain();

            return result;
        }

        public async Task<Models.TimeEntry> Handle(UpdateTimeEntry request, CancellationToken cancellationToken)
        {
            Models.TimeEntry result = request.Model;

            //// If the model does not have an EndWork date defined then it is attempting to activate
            //// a TimeEntry. We must make sure that there is not already an active TimeEntry for
            //// this user.
            ////
            //if (!result.EndWork.HasValue)
            //{
            //    Models.TimeEntry? activeTimeEntry = (this._mediator != null) ? 
            //        await this._mediator.Send(new GetActiveTimeEntry()) : null;

            //    if (activeTimeEntry != null)
            //    {
            //        var msg = $"There is already an active TimeEntry for {activeTimeEntry.WorkItem?.Title}. Close this entry before beginning another.";
            //        throw new SystemException(msg);
            //    }
            //}

            // Otherwise go ahead and update the entity from the model
            //
            DataAccess.Entities.TimeEntry? entity = await this._dbCtx.TimeEntries.FirstOrDefaultAsync(e => e.Id == request.Model.Id);

            if (entity != null)
            {
                entity.StartWork = request.Model.StartWork?.ConvertLocalToUtc();
                entity.EndWork = request.Model.EndWork?.ConvertLocalToUtc();
                entity.Accomplishment = request.Model.Accomplishment;
                entity.WorkItemId = request.Model.WorkItemId;
                entity.UpdatedDate = DateTime.UtcNow;
                entity.UpdatedBy = request.User.Username;

                await this._dbCtx.SaveChangesAsync();
                result = entity.MapEntityToDomain();
            }

            return result;
        }

        #endregion public
    }

    public record CreateTimeEntry(Models.TimeEntry Model, Models.Laborer User) : IRequest<Models.TimeEntry>;

    public record DeleteTimeEntry(Models.TimeEntry Model, string Username) : IRequest<bool>;

    public record EndWorkOnWorkItem(Models.TimeEntry Model, string Username, bool SubmittedForReview = false, bool CloseWorkItem = false) : IRequest<Models.TimeEntry?>;

    public record StartWorkOnWorkItem(Models.TimeEntry Model, Models.Laborer User) : IRequest<Models.TimeEntry>;

    public record UpdateTimeEntry(Models.TimeEntry Model, Models.Laborer User) : IRequest<Models.TimeEntry>;
}
