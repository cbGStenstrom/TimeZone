using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Services;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.App.Components.Forms
{
    public partial class CreateUserForm : ComponentBase
    {
        #region injected services

        [Inject]
        public ILaborerService? LaborerSvc { get; set; }

        #endregion injected services

        #region parameters

        [Parameter]
        public Laborer? User { get; set; }

        [Parameter]
        public EventCallback<Laborer> UserChanged { get; set; }

        #endregion parameters

        #region properties
        #endregion properties

        #region events
        #endregion events

        #region data
        #endregion data

        #region lifecycle

        protected override void OnInitialized()
        {
            // Initialize the User 
            //
            this.User = new Laborer();
            base.OnInitialized();
        }

        #endregion lifecycle

        #region event handlers
        #endregion event handlers

        #region private
        #endregion private

        #region public
        #endregion public
    }
}
