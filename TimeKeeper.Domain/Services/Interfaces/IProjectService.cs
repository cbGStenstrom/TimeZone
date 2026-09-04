using TimeKeeper.Domain.Models;

namespace TimeKeeper.Domain.Services.Interfaces
{
    public interface IProjectService
    {
        /// <summary>
        ///  Creates a new project and associates it with the specified user.
        /// </summary>
        /// <param name="model">
        ///  The project details to be added. Cannot be null.
        /// </param>
        /// <param name="user">
        ///  The laborer to whom the project will be assigned. Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result contains the created 
        ///  project instance.
        /// </returns>
        Task<Models.Project> AddProject(Models.Project model, Models.Laborer user);

        /// <summary>
        /// Permanently deletes the Project associated with the <paramref name="projectID"/> argument.
        /// from the database.
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns>Domain model of the deleted project</returns>
        Task<Project?> DeleteProject(int projectID);

        /// <summary>
        /// Returns the Project associated with the <paramref name="projectID"/> argument.
        /// </summary>
        /// <param name="projectID"></param>
        /// <returns></returns>
        Task<Project?> GetProjectByID(int projectID);

        /// <summary>
        /// Returns a list of Projects
        /// </summary>
        /// <returns></returns>
        Task<List<Project>> GetProjects();

        /// <summary>
        ///  Updates the specified project with new information provided by the user.
        /// </summary>
        /// <param name="model">
        ///  The project entity containing the updated values to apply. Cannot be null.
        /// </param>
        /// <param name="user">
        ///  The laborer performing the update operation. Determines permissions and audit information. 
        ///  Cannot be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result is <see langword="true"/> 
        ///  if the project was successfully updated; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> UpdateProject(Project model, Laborer user);

    }
}