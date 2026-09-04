using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TimeKeeper.DataAccess.Entities;

public partial class TimeKeeperDbContext : DbContext
{
    public TimeKeeperDbContext()
    {
    }

    public TimeKeeperDbContext(DbContextOptions<TimeKeeperDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Laborer> Laborers { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<TimeEntry> TimeEntries { get; set; }

    public virtual DbSet<WorkItem> WorkItems { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TimeKeeperDB;Integrated Security=True;Connect Timeout=60;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Laborer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Laborers__3214EC27142B3CEA");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(120);
            entity.Property(e => e.FirstName).HasMaxLength(120);
            entity.Property(e => e.LastName).HasMaxLength(120);
            entity.Property(e => e.MiddleName).HasMaxLength(120);
            entity.Property(e => e.Password).HasMaxLength(120);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(120);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC27741ABD8C");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Key)
                .HasMaxLength(20)
                .HasColumnName("KEY");
            entity.Property(e => e.LongName).HasMaxLength(250);
            entity.Property(e => e.ShortName).HasMaxLength(120);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC276236677A");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndWork).HasColumnType("datetime");
            entity.Property(e => e.LaborerId).HasColumnName("LaborerID");
            entity.Property(e => e.StartWork).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WorkItemId).HasColumnName("WorkItemID");

            entity.HasOne(d => d.Laborer).WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.LaborerId)
                .HasConstraintName("FK_TimeEntries_ToLaborers");

            entity.HasOne(d => d.WorkItem).WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.WorkItemId)
                .HasConstraintName("FK_TimeEntries_ToWorkItems");
        });
        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC2704B6B003");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsOpen).HasDefaultValue(true);
            entity.Property(e => e.ProductionVersion).HasMaxLength(20);
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.ReviewDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.WorkItemType).HasDefaultValue(0);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(120)
                .HasDefaultValue("AUTO");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Project).WithMany(p => p.WorkItems)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_WorkItems_ToProjects");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
