using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Queries
{
    public class GetProjectsQueryHandler(TimeKeeperDbContext dbCtx) : 
                            IRequestHandler<GetProjectQuery, Models.Project?>,
                            IRequestHandler<GetFilteredProjectsQuery, List<Models.Project>>
    {
        #region properties/fields

        private readonly TimeKeeperDbContext _dbCtx = dbCtx;

        #endregion properties/fields

        #region ctor
        #endregion ctor

        #region handlers

        /// <summary>
        ///  Returns a list of optionally filtered Project objects where the filter is defined in 
        ///  the Filter property of the <paramref name="request"/> argument
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<Models.Project>> Handle(GetFilteredProjectsQuery request, CancellationToken cancellationToken)
        {
            List<Project> entities = (request.Filter != null) ? await _dbCtx.Projects.Where(request.Filter).ToListAsync(cancellationToken) :
                                                               await _dbCtx.Projects.ToListAsync(cancellationToken);

            List<Models.Project> results = new List<Models.Project>();
            foreach (var entity in entities)
            {
                Models.Project projectModel = entity.MapEntityToDomain();
                projectModel.TakeSnapshot();
                results.Add(projectModel);
            }

            return results;
        }

        /// <summary>
        ///  Returns the single Project associated with the ProjectID property of the <paramref name="request"/> 
        ///  argument.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Models.Project?> Handle(GetProjectQuery request, CancellationToken cancellationToken)
        {
            Project? entity = await _dbCtx.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectID, cancellationToken);

            Models.Project? result = entity != null ? entity.MapEntityToDomain() : null;
            result?.TakeSnapshot();


            return result;
        }

        #endregion handlers
    }

    /// <summary>
    ///  Returns a single project associated with the <paramref name="ProjectID"/> argument.
    /// </summary>
    /// <param name="ProjectID"></param>
    public record GetProjectQuery(int ProjectID) : IRequest<Models.Project?>;

    /// <summary>
    /// Returns a filtered list of projects 
    /// </summary>
    public record GetFilteredProjectsQuery(Expression<Func<Project, bool>>? Filter = null) : IRequest<List<Models.Project>>;

}
