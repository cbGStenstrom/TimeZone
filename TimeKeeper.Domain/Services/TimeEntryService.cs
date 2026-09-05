using System.Linq.Expressions;
using MediatR;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.Domain.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        #region fields

        IMediator? _mediator = null;

        #endregion fields

        #region properties
        #endregion properties

        #region data
        #endregion data

        #region ctor

        public TimeEntryService(TimeEntryServiceOptions options)
        {
            this._mediator = options.Mediator;
        }

        #endregion ctor

        #region private
        #endregion private

        #region Queries

        /// <summary>
        ///  Retrieves the currently active time entry, if one exists.
        /// </summary>
        /// <returns>
        ///  A <see cref="Models.TimeEntry"/> representing the active time entry. If there is no 
        ///  active time entry, returns a new <see cref="Models.TimeEntry"/> instance with default 
        ///  values.
        /// </returns>
        public async Task<Models.TimeEntry?> GetActiveTimeEntry()
        {
            Models.TimeEntry? result = (this._mediator != null) ?
                                      await this._mediator.Send(new GetActiveTimeEntry()) : null;
            return result;

        }

        /// <summary>
        /// Returns an optionally filtered list of TimeEntry objects. 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Models.TimeEntry>> GetFilteredTimeEntries(List<Expression<Func<DataAccess.Entities.TimeEntry, bool>>>? filter = null)
        {
            IEnumerable<Models.TimeEntry> result = (this._mediator != null) ?
                                                await this._mediator.Send(new GetFilteredTimeEntries(filter)) :
                                                new List<Models.TimeEntry>();

            foreach(var item in result) { item.TakeSnapshot(); }
            return result;
        }

        /// <summary>
        ///  Retrieves a time entry with the specified identifier.
        /// </summary>
        /// <param name="timeEntryId">
        ///  The unique identifier of the time entry to retrieve. Must be a positive integer.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result contains the 
        ///  <see cref="Models.TimeEntry"/> with the specified identifier, or a new <see cref="Models.TimeEntry"/> 
        ///  instance if not found.
        /// </returns>
        public async Task<Models.TimeEntry> GetTimeEntryById(int timeEntryId)
        {
            Models.TimeEntry result = (this._mediator != null) ?
                                      await this._mediator.Send(new GetTimeEntryById(timeEntryId)) :
                                      new ();

            return result;
        }

        /// <summary>
        ///  Asynchronously gets the number of time entries associated with a work item. 
        /// </summary>
        /// <param name="workItemId">
        ///  The identifier of the work item.</param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result contains the number 
        ///  of matching time entries.</returns>
        public async Task<int> GetTimeEntryCountForWorkItem(int workItemId)
        {
            IEnumerable<Models.TimeEntry> records =
                await this.GetFilteredTimeEntries(
                    [
                        e => e.WorkItemId == workItemId
                    ]);

            return records.Count();
        }

        #endregion Queries

        #region Commands

        /// <summary>
        ///  Creates a new time entry for the specified laborer.
        /// </summary>
        /// <param name="model">
        ///  The time entry data to create. Must not be null.
        /// </param>
        /// <param name="user">
        ///  The laborer for whom the time entry is being created. Must not be null.
        /// </param>
        /// <returns>
        ///  A task that represents the asynchronous operation. The task result contains the created 
        ///  time entry if successful; otherwise, null.
        /// </returns>
        public async Task<Models.TimeEntry?> CreateNewTimeEntry(Models.TimeEntry model, Models.Laborer user)
        {
            Models.TimeEntry?  result = (this._mediator != null) ?
                                         await this._mediator.Send(new CreateTimeEntry(model, user)) :
                                         null;

            return result;
        }

        /// <summary>
        ///  Permanently deletes the <paramref name="model"/> argument from the database.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTimeEntry(Models.TimeEntry model, String username)
        {
            bool result = (this._mediator != null) ?
                          await this._mediator.Send(new DeleteTimeEntry(model, username)) :
                          false;

            return result;
        }

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
        public async Task<Models.TimeEntry?> EndWorkOnWorkItem(Models.TimeEntry Model, String Username, bool SubmitForReview = false, bool CloseWorkitem = false)
        {
            Models.TimeEntry? result = (this._mediator != null) ?
                                      await this._mediator.Send(new EndWorkOnWorkItem( Model, Username, SubmitForReview, CloseWorkitem)) :
                                      null;

            return result;
        }

        /// <summary>
        ///  Starts tracking work on the specified work item for the given laborer.
        /// </summary>
        /// <param name="model">
        ///  The time entry model representing the work item to start tracking. Must not be null.
        /// </param>
        /// <param name="User">
        ///  The laborer for whom the work is being started. Must not be null.
        /// </param>
        /// <returns> 
        ///  A task that represents the asynchronous operation. The task result contains a 
        ///  <see cref="Models.TimeEntry"/> object representing the started work entry.
        /// </returns>
        public async Task<Models.TimeEntry> StartWorkOnWorkItem(Models.TimeEntry model, Models.Laborer User)
        {
            Models.TimeEntry result = (this._mediator != null) ?
                                      await this._mediator.Send(new StartWorkOnWorkItem(model, User)) :
                                      new Models.TimeEntry();

            return result;
        }

        /// <summary>
        /// Saves updates to an existing TimeEntry to the underlying datastore
        /// </summary>
        /// <param name="model"></param>
        /// <param name="User"></param>
        /// <returns></returns>
        public async Task<Models.TimeEntry> UpdateTimeEntry(Models.TimeEntry model, Models.Laborer User)
        {
            Models.TimeEntry result = (this._mediator != null) ?
                                      await this._mediator.Send(new UpdateTimeEntry(model, User)) :
                                      new Models.TimeEntry();

            return result;
        }

        #endregion Commands
    }

    public record TimeEntryServiceOptions(IMediator Mediator);
}
