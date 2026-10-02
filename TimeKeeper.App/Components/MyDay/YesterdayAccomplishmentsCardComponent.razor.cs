using Microsoft.AspNetCore.Components;
using TimeKeeper.Domain.Models;

namespace TimeKeeper.App.Components.MyDay
{
    public partial class YesterdayAccomplishmentsCardComponent : ComponentBase
    {
        #region injected services
        #endregion injected services

        #region parameters

        [Parameter]
        public IEnumerable<TimeEntry> TimeEntries { get; set; } = [];

        #endregion parameters

        #region properties
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
        #endregion public
    }
}