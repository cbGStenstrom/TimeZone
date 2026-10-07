using Microsoft.EntityFrameworkCore;
namespace TimeKeeper.DataAccess.Entities;

public partial class TimeKeeperDbContext
{
    public virtual DbSet<WorkItemType> WorkItemTypes { get; set; } = null!;
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkItemType>(e =>
        {
            e.ToTable("WorkItemTypes", t => t.HasCheckConstraint("CK_WorkItemTypes_Name", "LEN(LTRIM(RTRIM(Name))) > 0"));
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).UseIdentityColumn();
            e.Property(t => t.Name).IsRequired().HasMaxLength(120).UseCollation("Latin1_General_100_CI_AS");
            e.HasIndex(t => t.Name).IsUnique().HasDatabaseName("UQ_WorkItemTypes_Name");
            e.Property(t => t.Description).HasMaxLength(1000);
            e.Property(t => t.Icon).HasMaxLength(120);
            e.Property(t => t.CreatedBy).IsRequired().HasMaxLength(120).HasDefaultValue("AUTO");
            e.Property(t => t.UpdatedBy).IsRequired().HasMaxLength(120).HasDefaultValue("AUTO");
            e.Property(t => t.CreatedDate).HasColumnType("datetime").HasDefaultValueSql("getutcdate()");
            e.Property(t => t.UpdatedDate).HasColumnType("datetime").HasDefaultValueSql("getutcdate()");
        });
        modelBuilder.Entity<WorkItem>().Property(w => w.WorkItemType).HasDefaultValue((object?)null).ValueGeneratedNever();
        modelBuilder.Entity<WorkItem>().HasOne(w => w.Type).WithMany(t => t.WorkItems)
         .HasForeignKey(w => w.WorkItemType).IsRequired().OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<WorkItem>().HasIndex(w => w.WorkItemType);
    }
}
