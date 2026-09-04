using MediatR;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.Domain.Services
{
    public class ProjectService : IProjectService
    {
        #region fields

        IMediator? _mediator = null;

        #endregion fields

        #region properties
        #endregion properties

        #region data
        #endregion data

        #region ctor

        public ProjectService(ProjectServiceOptions options)
        {
            this._mediator = options.Mediator;
        }

        #endregion ctor

        #region private
        #endregion private

        #region public

        /// <summary>
        ///  Creates a new project and associates it with the specified user.
        /// </summary>
        /// <param name="model">
        ///  The project details to be added. Must contain valid project information.
        /// </param>
        /// <param name="user">
        ///  The laborer to associate with the new project. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result contains the created project instance.
        /// </returns>
        public async Task<Models.Project> AddProject(Models.Project model, Models.Laborer user)
        {
            ArgumentNullException.ThrowIfNull(user);

            Models.Project result = (this._mediator != null) ? await this._mediator.Send(new AddProject(model, user)) : model;
            return result;
        }

        /// <summary>
        /// Permanently deletes the Project associated with the <paramref name="projectID"/> argument.
        /// from the database.
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns>Domain model of the deleted project</returns>
        public async Task<Models.Project?> DeleteProject(int projectID)
        {
            Models.Project? result = (this._mediator != null) ? await this._mediator.Send(new DeleteProject(projectID)) : null;
            return result;
        }

        /// <summary>
        /// Returns the Project associated with the <paramref name="projectID"/> argument.
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns></returns>
        public async Task<Models.Project?> GetProjectByID(int projectID)
        {
            Models.Project? result = (this._mediator != null) ? await this._mediator.Send(new GetProjectQuery(projectID)) : null;
            return result;
        }

        /// <summary>
        /// Returns a list of Projects
        /// </summary>
        /// <returns></returns>
        public async Task<List<Models.Project>> GetProjects()
        {
            List<Models.Project> result = (this._mediator != null) ? await this._mediator.Send(new GetFilteredProjectsQuery()) : new List<Models.Project>();
            return result;
        }

        /// <summary>
        ///  Updates the specified project with new information provided by the user.
        /// </summary>
        /// <remarks>
        ///  If the update operation cannot be performed, such as when the mediator is unavailable, 
        ///  the method returns <see langword="false"/>.
        /// </remarks>
        /// <param name="model">
        ///  The project data to be updated. Cannot be null.
        /// </param>
        /// <param name="user">
        ///  The user performing the update operation. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result is <see langword="true"/> 
        ///  if the project was updated successfully; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<bool> UpdateProject(Models.Project model, Laborer user)
        {
            bool result = (this._mediator != null) ? await this._mediator.Send(new UpdateProject(model, user)) : false;
            return result;
        }

        #endregion public
    }

    public record ProjectServiceOptions(IMediator Mediator);
}
