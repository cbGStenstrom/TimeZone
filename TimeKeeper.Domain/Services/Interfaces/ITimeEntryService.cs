using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;

namespace TimeKeeper.Domain.Services.Interfaces
{
    public interface ITimeEntryService
    {
        Task<Models.TimeEntry?> CreateNewTimeEntry(Models.TimeEntry model, Models.Laborer user);

        Task<bool> DeleteTimeEntry(Models.TimeEntry model, String username);


        /// <summary>
        ///  Ends the specified time entry for the given user and optionally submits it for review 
        ///  or closes the associated work item.
        /// </summary>
        /// <param name="Model">
        ///  The time entry to be ended. Cannot be null.
        /// </param>
        /// <param name="Username">
        ///  The username of the user ending the time entry. Cannot be null or empty.
        /// </param>
        /// <param name="SubmitForReview">
        ///  Indicates whether the time entry should be submitted for review after ending. Set to 
        ///  <see langword="true"/> to submit for review; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="CloseWorkitem">
        ///  Indicates whether the associated work item should be closed after ending the time entry. 
        ///  Set to <see langword="true"/> to close the work item; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        ///  A <see cref="Models.TimeEntry"/> representing the ended time entry, or <see langword="null"/> 
        ///  if the operation could not be completed.
        /// </returns>
        Task<Models.TimeEntry?> EndWorkOnWorkItem(Models.TimeEntry Model, String Username, bool SubmitForReview = false, bool CloseWorkitem = false);

        Task<Models.TimeEntry?> GetActiveTimeEntry();

        Task<IEnumerable<Models.TimeEntry>> GetFilteredTimeEntries(List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>? filter = null);

        Task<Models.TimeEntry> GetTimeEntryById(int timeEntryId);

        Task<Models.TimeEntry> StartWorkOnWorkItem(Models.TimeEntry model, Models.Laborer User);

        Task<Models.TimeEntry> UpdateTimeEntry(Models.TimeEntry model, Models.Laborer User);

    }
}