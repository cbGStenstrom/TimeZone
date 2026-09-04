using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Api.Services
{
    public class SessionService
    {
        #region injected services
        #endregion injected services

        #region parameters
        #endregion parameters

        #region data-bound parameters
        #endregion data-bound parameters

        #region properties

        /// <summary>
        /// Gets/Sets the actively logged in user
        /// </summary>
        public Laborer? User { get; set; }

        #endregion properties

        #region events
        #endregion events

        #region data
        #endregion data

        #region lifecycle
        #endregion lifecycle

        #region event handlers
        #endregion event handlers

        #region private
        #endregion private

        #region public

        public void Logout()
        {
            this.User = null;
        }

        #endregion public
    }
}
