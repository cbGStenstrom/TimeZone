using MediatR;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;
namespace TimeKeeper.Domain.Services;

public class WorkItemTypeService(IMediator mediator) : IWorkItemTypeService
{
    /// <summary>
    ///  Deletes the work item type with the specified identifier.
    /// </summary>
    /// <param name="id">
    ///  The identifier of the work item type to delete.</param>
    /// <returns>
    ///  A task that represents the asynchronous delete operation.</returns>
    public async Task Delete(int id) => await mediator.Send(new DeleteWorkItemType(id));

    /// <summary>
    ///  Gets work item types.
    /// </summary>
    /// <param name="enabledOnly">
    ///  true to return only enabled work item types; otherwise, all work item types.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation. The task result contains the list of 
    ///  work item types.</returns>
    public Task<List<WorkItemType>> GetTypes(bool enabledOnly = false) => mediator.Send(new GetWorkItemTypes(enabledOnly));

    /// <summary>
    ///  Saves the specified work item type for the specified user.
    /// </summary>
    /// <param name="model">
    ///  Work item type to save.</param>
    /// <param name="user">
    ///  User performing the save operation.</param>
    /// <returns>
    ///  A task that represents the asynchronous save operation.</returns>
    public async Task Save(WorkItemType model, Laborer user) => await mediator.Send(new SaveWorkItemType(model, user));

    /// <summary>
    /// Sets whether the specified work item type is enabled.
    /// </summary>
    /// <param name="id">
    ///  The identifier of the work item type.</param>
    /// <param name="enabled">
    ///  A value indicating whether the work item type is enabled.</param>
    /// <param name="user">
    ///  The user performing the operation.</param>
    /// <returns>
    ///  A task that represents the asynchronous operation.</returns>
    public async Task SetEnabled(int id, bool enabled, Laborer user) => await mediator.Send(new SetWorkItemTypeEnabled(id, enabled, user));
}
