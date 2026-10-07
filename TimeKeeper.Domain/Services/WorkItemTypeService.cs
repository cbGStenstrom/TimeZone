using MediatR;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;
namespace TimeKeeper.Domain.Services;

public class WorkItemTypeService(IMediator mediator) : IWorkItemTypeService
{
    public Task<List<WorkItemType>> GetTypes(bool enabledOnly = false) => mediator.Send(new GetWorkItemTypes(enabledOnly));
    public async Task Save(WorkItemType model, Laborer user) => await mediator.Send(new SaveWorkItemType(model, user));
    public async Task SetEnabled(int id, bool enabled, Laborer user) => await mediator.Send(new SetWorkItemTypeEnabled(id, enabled, user));
    public async Task Delete(int id) => await mediator.Send(new DeleteWorkItemType(id));
}
