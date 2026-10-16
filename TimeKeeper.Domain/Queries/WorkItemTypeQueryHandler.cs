using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Utilities.Mappers;
namespace TimeKeeper.Domain.Queries;

public record GetWorkItemTypes(bool EnabledOnly = false) : IRequest<List<Models.WorkItemType>>;
public class WorkItemTypeQueryHandler(TimeKeeperDbContext ctx) : IRequestHandler<GetWorkItemTypes, List<Models.WorkItemType>>
{
    public async Task<List<Models.WorkItemType>> Handle(GetWorkItemTypes request, CancellationToken ct)
    {
        var rows = await ctx.WorkItemTypes.AsNoTracking().Where(t => !request.EnabledOnly || t.IsEnabled)
         .OrderBy(t => t.Name).Select(t => new { Type = t, Count = t.WorkItems.Count }).ToListAsync(ct);
        return rows.Select(r => { var m = r.Type.MapEntityToDomain(); m.ActivityCount = r.Count; return m; }).ToList();
    }
}
