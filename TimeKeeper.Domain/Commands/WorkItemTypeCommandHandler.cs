using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TimeKeeper.DataAccess.Entities;
namespace TimeKeeper.Domain.Commands;

public record SaveWorkItemType(Models.WorkItemType Model, Models.Laborer User) : IRequest<bool>;
public record SetWorkItemTypeEnabled(int Id, bool Enabled, Models.Laborer User) : IRequest<bool>;
public record DeleteWorkItemType(int Id) : IRequest<bool>;
public class WorkItemTypeCommandHandler(TimeKeeperDbContext ctx) :
 IRequestHandler<SaveWorkItemType, bool>, IRequestHandler<SetWorkItemTypeEnabled, bool>, IRequestHandler<DeleteWorkItemType, bool>
{
    public async Task<bool> Handle(SaveWorkItemType r, CancellationToken ct)
    {
        var m = r.Model; m.Validate();
        if (await ctx.WorkItemTypes.AnyAsync(t => (m.IsNew || t.Id != m.Id) && t.Name == m.Name, ct))
            throw new InvalidOperationException("An Activity Type with this name already exists.");
        var entity = m.IsNew ? new WorkItemType() : await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == m.Id, ct)
         ?? throw new InvalidOperationException("Activity Type no longer exists. Refresh the list.");
        if (!m.IsNew && entity.Name == m.Name && entity.Description == m.Description && entity.Icon == m.Icon && entity.IsEnabled == m.IsEnabled) return false;
        if (m.IsNew) { entity.CreatedDate = DateTime.UtcNow; entity.CreatedBy = r.User.Username; ctx.WorkItemTypes.Add(entity); }
        entity.Name = m.Name; entity.Description = m.Description; entity.Icon = m.Icon; entity.IsEnabled = m.IsEnabled;
        entity.UpdatedDate = DateTime.UtcNow; entity.UpdatedBy = r.User.Username;
        await Save(ct); m.Id = entity.Id; m.IsNew = false; return true;
    }
    public async Task<bool> Handle(SetWorkItemTypeEnabled r, CancellationToken ct)
    {
        var entity = await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == r.Id, ct)
         ?? throw new InvalidOperationException("Activity Type no longer exists. Refresh the list.");
        if (entity.IsEnabled == r.Enabled) return false;
        entity.IsEnabled = r.Enabled; entity.UpdatedDate = DateTime.UtcNow; entity.UpdatedBy = r.User.Username;
        await Save(ct); return true;
    }
    public async Task<bool> Handle(DeleteWorkItemType r, CancellationToken ct)
    {
        var entity = await ctx.WorkItemTypes.SingleOrDefaultAsync(t => t.Id == r.Id, ct);
        if (entity == null) return false;
        if (await ctx.WorkItems.AnyAsync(w => w.WorkItemType == r.Id, ct))
            throw new InvalidOperationException("Activities now reference this type. Refresh and confirm disabling it instead.");
        ctx.WorkItemTypes.Remove(entity); await Save(ct); return true;
    }
    async Task Save(CancellationToken ct)
    {
        try { await ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627 || sql.Number == 547))
        {
            ctx.ChangeTracker.Clear();
            throw new InvalidOperationException(sql.Number == 547 ? "Activities reference this type. Refresh and confirm disabling it instead." : "An Activity Type with this name already exists.", ex);
        }
        catch
        {
            // A failed or canceled write must not be retried by a later operation.
            ctx.ChangeTracker.Clear();
            throw;
        }
    }
}
