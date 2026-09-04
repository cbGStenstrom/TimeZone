using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Commands
{
    public class ProjectCommandHandler(TimeKeeperDbContext ctx) :
                IRequestHandler<AddProject, Models.Project>,
                IRequestHandler<DeleteProject, Models.Project>,
                IRequestHandler<UpdateProject, bool>
    {
        #region properties

        TimeKeeperDbContext _dbCtx = ctx;

        #endregion properties

        #region ctor
        #endregion ctor

        #region handlers

        /// <summary>
        /// Creates a new Peoject record in the database.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.Project> Handle(AddProject request, CancellationToken cancellationToken)
        {
            Project entity      = request.Model.MapDomainToEntity();
            entity.CreatedDate  = entity.UpdatedDate = DateTime.UtcNow;
            entity.CreatedBy    = entity.UpdatedBy   = request.User.Username;

            await this._dbCtx.Projects.AddAsync(entity, cancellationToken);
            await this._dbCtx.SaveChangesAsync(cancellationToken);

            Models.Project result = entity.MapEntityToDomain();
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Permanently deletes a Project from the database.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.Project?> Handle(DeleteProject request, CancellationToken cancellationToken)
        {
            Models.Project? result = null;

            DataAccess.Entities.Project? entity = await this._dbCtx.Projects.FirstOrDefaultAsync(e=>e.Id == request.ProjectID, cancellationToken);
            if (entity != null)
            {
                await Task.FromResult(this._dbCtx.Projects.Remove(entity));
                await this._dbCtx.SaveChangesAsync(cancellationToken);

                result = entity.MapEntityToDomain();
                result.Id = 0;
            }

            return result;
        }

        /// <summary>
        /// Updates the values of a Project record.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(UpdateProject request, CancellationToken cancellationToken)
        {
            Project? entity = await this._dbCtx.Projects.FirstOrDefaultAsync(e=>e.Id == request.Model.Id);

            if (entity == null)
            {
                return false;
            }

            await Task.Run(() => { request.Model.MapDomainToEntity(ref entity); });
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy   = request.User.Username;

            return (await this._dbCtx.SaveChangesAsync(cancellationToken)) > 0 ;
        }

        #endregion handlers
    }

    /// <summary>
    ///  Creates a new Project record
    /// </summary>
    /// <param name="Model"></param>
    public record AddProject(Models.Project Model, Models.Laborer User) : IRequest<Models.Project>;

    /// <summary>
    ///  Permanently deletes the Project associated with the <paramref name="ProjectID"/> argument 
    ///  from the database.
    /// </summary>
    /// <param name="ProjectID"></param>
    public record DeleteProject(int ProjectID) : IRequest<Models.Project>;

    /// <summary>
    /// Updates an existing project record.
    /// </summary>
    /// <param name="Model"></param>
    public record UpdateProject(Models.Project Model, Models.Laborer User) : IRequest<bool>;
}
