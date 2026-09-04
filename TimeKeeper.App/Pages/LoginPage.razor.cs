using Microsoft.AspNetCore.Components;
using TimeKeeper.App.Api.Services;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Pages
{
    public partial class LoginPage : ComponentBase
    {
        #region injected services

        [Inject]
        public ILaborerService? LaborerService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public SessionService? SessionService { get; set; }

        #endregion injected services

        #region parameters
        #endregion parameters

        #region properties

        bool IsBusy { get; set; } = false;

        string? Password { get; set; }

        string? Username { get; set; }

        string? UserMessage { get; set; }

        bool ShowCreateUser { get; set; } = false;

        string Title { get; set; } = "Login";

        Laborer EditableUser { get; set; } = new Laborer();

        #endregion properties

        #region events
        #endregion events

        #region data

        async Task SaveNewUser()
        {
            await (this.LaborerService?.CreateUser(this.EditableUser) ?? Task.CompletedTask);
        }

        #endregion data

        #region lifecycle

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        #endregion lifecycle

        #region event handlers

        void btnCancelCreateUser_OnClick()
        {
            this.ShowCreateUser = false;
            this.UserMessage = null;
            this.Title = "Login";
            this.StateHasChanged();
        }

        void  btnCreateUser_OnClick()
        {
            this.UserMessage = null;
            this.EditableUser = new Laborer();
            this.ShowCreateUser = true;
            this.Title = "Create New Account";
            this.StateHasChanged();
        }

        async Task btnLogin_OnClick()
        {
            this.IsBusy = true;

            if(this.LaborerService != null)
            {
                try
                {

                    Laborer? user = (!string.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password)) ? 
                                    await LaborerService.GetFromCredentials(this.Username, this.Password) : 
                                    null;

                    if (user == null)
                    {
                        this.UserMessage = "Login Failed";
                        this.IsBusy = false;
                    }
                    else
                    {
                        if (SessionService != null && NavigationManager != null)
                        {
                            SessionService.User = user;
                            NavigationManager.NavigateTo("/");
                        }
                    }
                }
                #pragma warning disable CS0168
                catch(Exception ex)
                #pragma warning restore CS0168
                {
                    UserMessage = $"An error occurred while logging in.: {ex.Message}";
                    this.IsBusy = false;
                }
            }
        }

        async Task btnSaveUser_OnClick()
        {
            try
            {
                await this.SaveNewUser();
                this.ShowCreateUser = false;
                this.UserMessage = "Your account was successfully created.";
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) { ex = ex.InnerException; }
                this.UserMessage = ex.Message;
            }
        }

        #endregion event handlers

        #region private
        #endregion private

        #region public
        #endregion public
    }
}
