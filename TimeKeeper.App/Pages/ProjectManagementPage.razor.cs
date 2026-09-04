using TimeKeeper.App.Components.Pages;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Pages
{
    public partial class ProjectManagementPage : CbPageBase
    {
        #region injected services
        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties

        /// <summary>
        /// Gets a flag indicating whether the Delete button should be disabled.
        /// </summary>
        bool DeleteDisabled { get {  return !SelectedProjectID.HasValue; } }

        /// <summary>
        /// Gets/Sets the list of projects available in the Drop Down List
        /// </summary>
        List<Project> ProjectList { get; set; } = new List<Project>();

        /// <summary>
        /// Get/Sets the project to be loaded into the edit form.
        /// </summary>
        Project SelectedProject { get; set; } = new Project();

        /// <summary>
        ///  Gets/Sets the ID of the project loaded in the edit form
        /// </summary>
        int? SelectedProjectID { get; set; }

        /// <summary>
        /// Gets a flag indicating whether the btnSave_OnClick button should be disabled.
        /// </summary>
        bool SaveDisabled { get { return !SelectedProjectID.HasValue; } }

        #endregion properties

        #region events
        #endregion events

        #region data

        /// <summary>
        /// Permanently deletes the SelectedProject from the database.
        /// </summary>
        /// <returns></returns>
        async Task<Project?> DeleteProject()
        {
            Project? result = (await base.ProjectSvc!.DeleteProject(this.SelectedProject.Id)) ?? null;
            return result;
        }

        /// <summary>
        ///  Populates the ProjectList property which is bound to the Projects Drop Down List
        /// </summary>
        /// <returns></returns>
        async Task LoadProjectList()
        {
            this.ProjectList = (await base.ProjectSvc!.GetProjects()) ?? new List<Project>();
        }

        /// <summary>
        /// Saves the Selected Project. Updates it if it already exists, but adds it if it does not.
        /// </summary>
        /// <returns></returns>
        async Task<bool> SaveProject()
        {
            bool success = false;
            ArgumentNullException.ThrowIfNull(base.SessionSvc?.User);

            if (this.ProjectSvc != null)
            {
                if (this.SelectedProject.Id == default(int))
                {
                    this.SelectedProject = await this.ProjectSvc.AddProject(this.SelectedProject, base.SessionSvc.User);
                    success = true;

                }
                else
                {
                    success = await this.ProjectSvc.UpdateProject(this.SelectedProject, base.SessionSvc.User);
                }
            }

            return success;
        }

        #endregion data

        #region lifecycle

        protected override async Task OnInitializedAsync()
        {
            await this.LoadProjectList();
        }

        #endregion lifecycle

        #region event handlers

        void btnCancelProject_OnClick()
        {
            this.ClearProjectForm();
            this.StateHasChanged();
        }

        async Task btnDeleteProject_OnClick()
        {
            await this.DeleteProject();
            this.SelectedProject = new Project();
            this.SelectedProjectID = null;

            await this.LoadProjectList();
            base.StateHasChanged();
        }

        void btnNewProject_OnClick()
        {
            this.SelectedProject = new Project()
            {
                CreatedBy = this.SessionSvc?.User?.Username ?? "auto",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = this.SessionSvc?.User?.Username ?? "auto",
                UpdatedDate = DateTime.UtcNow
            };
            this.SelectedProjectID = this.SelectedProject.Id;
            this.StateHasChanged();
        }

        async Task btnSaveProject_OnClick()
        {
            if(await this.SaveProject())
            {
                await this.LoadProjectList();
                this.ClearProjectForm();

                this.StateHasChanged();
            }
        }

        void ddlProjects_OnChange()
        {
            this.SelectedProject = (this.ProjectList?.FirstOrDefault(e=>e.Id == this.SelectedProjectID)) ?? new Project();
            //this.SelectedProject = (this.SelectedProject == null) ? new Project() : this.SelectedProject;
            this.SelectedProjectID = this.SelectedProject.Id;
            this.StateHasChanged();
        }

        #endregion event handlers

        #region private

        void ClearProjectForm()
        {
            this.SelectedProject = new Project();
            this.SelectedProjectID = null;
        }

        #endregion private

        #region public
        #endregion public
    }
}
