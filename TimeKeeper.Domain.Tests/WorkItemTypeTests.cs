using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Utilities.Mappers;
using Xunit;
using Model = TimeKeeper.Domain.Models.WorkItemType;
using TimeKeeper.Domain.Models;
namespace TimeKeeper.Domain.Tests;

public class WorkItemTypeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    public void PersistedTypeMappingPreservesKeysAndOptionalFields(int id)
    {
        var entity = new DataAccess.Entities.WorkItemType
        {
            Id = id,
            Name = "Custom",
            Icon = "event",
            Description = null,
            IsEnabled = false,
            CreatedBy = "creator",
            UpdatedBy = "editor",
            CreatedDate = new DateTime(2026, 1, 1),
            UpdatedDate = new DateTime(2026, 2, 1)
        };
        var model = entity.MapEntityToDomain();
        Assert.False(model.IsNew); Assert.Equal(id, model.Id); Assert.False(model.IsEnabled);
        Assert.Equal(entity.CreatedDate, model.CreatedDate); Assert.Equal("creator", model.CreatedBy);
        Assert.Equal(entity.UpdatedDate, model.UpdatedDate); Assert.Equal("editor", model.UpdatedBy);
        Assert.Equal("event", model.Icon); Assert.Null(model.Description);
        var item = new DataAccess.Entities.WorkItem { WorkItemType = id, Type = entity };
        var domain = item.MapEntityToDomain();
        Assert.Equal(id, domain.WorkItemType); Assert.Equal("Custom", domain.Type!.Name);
        Assert.Equal(id, domain.MapDomainToEntity().WorkItemType);
    }
    [Fact]
    public void DraftChangesDoNotChangeOriginalIncludingLegacyZero()
    {
        var model = new Model { Id = 0, IsNew = false, Name = "Bugfix", IsEnabled = true };
        var draft = model.Draft(); draft.Name = "Edited"; draft.Icon = "event"; draft.IsEnabled = false;
        Assert.Equal("Bugfix", model.Name); Assert.Null(model.Icon); Assert.True(model.IsEnabled); Assert.False(draft.IsNew);
    }
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void BlankNamesAreRejected(string name) => Assert.Throws<InvalidOperationException>(() => new Model { Name = name }.Validate());
    [Fact]
    public void NameIsTrimmedAndOptionalFieldsAndDisabledInputAreRetained()
    {
        var model = new Model { Name = " Custom ", Description = null, Icon = "event", IsEnabled = false };
        model.Validate(); Assert.Equal("Custom", model.Name); Assert.Null(model.Description); Assert.Equal("event", model.Icon); Assert.False(model.IsEnabled);
    }
    [Fact]
    public void LimitsMatchSqlAndMissingWorkItemTypeIsNotBugfix()
    {
        Assert.Throws<InvalidOperationException>(() => new Model { Name = new string('a', 121) }.Validate());
        Assert.Throws<InvalidOperationException>(() => new Model { Name = "x", Description = new string('a', 1001) }.Validate());
        Assert.Throws<InvalidOperationException>(() => new Model { Name = "x", Icon = new string('a', 121) }.Validate());
        Assert.Throws<InvalidOperationException>(() => new TimeKeeper.Domain.Models.WorkItem().MapDomainToEntity());
    }
    [Fact]
    public void SnapshotRestoresTypeIdAndMetadata()
    {
        var original = new Model { Id = 0, IsNew = false, Name = "Bugfix", IsEnabled = false };
        var item = new TimeKeeper.Domain.Models.WorkItem { Id = 12, WorkItemType = 0, Type = original };
        item.TakeSnapshot(); Assert.False(item.IsDirty());
        item.WorkItemType = 17; item.Type = new Model { Id = 17, Name = "New" }; Assert.True(item.IsDirty());
        item.RevertToSnapshot(); Assert.Equal(0, item.WorkItemType); Assert.Same(original, item.Type); Assert.False(item.IsDirty());
    }
    [Theory]
    [InlineData(null, null, null, false, false)]
    [InlineData(0, null, 0, true, true)]
    [InlineData(17, null, 17, true, true)]
    [InlineData(17, null, null, true, false)]
    [InlineData(17, null, 17, false, false)]
    [InlineData(17, 0, 17, false, false)]
    [InlineData(0, 0, 0, false, true)]
    public void AssignmentRulesPreserveOnlyAnUnchangedDisabledAssociation(int? selected, int? previous, int? found, bool enabled, bool valid)
    {
        if (valid) WorkItemTypeRules.ValidateAssignment(selected, previous, found, enabled);
        else Assert.Throws<InvalidOperationException>(() => WorkItemTypeRules.ValidateAssignment(selected, previous, found, enabled));
    }
    [Fact]
    public void EfUsesRequiredRestrictiveRelationshipAndExplicitZeroForeignKey()
    {
        using var ctx = new TimeKeeperDbContext(new DbContextOptionsBuilder<TimeKeeperDbContext>().UseSqlServer("Server=unused;Database=unused;Integrated Security=true").Options);
        var item = ctx.Model.FindEntityType(typeof(DataAccess.Entities.WorkItem))!;
        var fk = Assert.Single(item.GetForeignKeys(), f => f.PrincipalEntityType.ClrType == typeof(DataAccess.Entities.WorkItemType));
        Assert.True(fk.IsRequired); Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior);
        Assert.Equal(ValueGenerated.Never, item.FindProperty("WorkItemType")!.ValueGenerated);
        Assert.Null(item.FindProperty("WorkItemType")!.FindAnnotation(RelationalAnnotationNames.DefaultValue));
        var type = ctx.Model.FindEntityType(typeof(DataAccess.Entities.WorkItemType))!;
        Assert.Contains(type.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "Name");
        Assert.Equal(120, type.FindProperty("Name")!.GetMaxLength()); Assert.False(type.FindProperty("Name")!.IsNullable);
    }
}
