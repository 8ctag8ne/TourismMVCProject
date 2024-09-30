using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace Tourism;

public partial class TourismDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public TourismDbContext()
    {
    }

    public TourismDbContext(DbContextOptions<TourismDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<GuideTour> GuideTours { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("text");
            entity.Property(e => e.MainPhoto).IsUnicode(false);
            entity.Property(e => e.Name).HasColumnType("text");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("text");

            entity.HasOne(d => d.Region).WithMany(p => p.Cities)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_Cities_Regions");
            entity.Property(e => e.Name).HasColumnType("text");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(e => e.Text).HasColumnType("text");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK_Comments_Comments");

            entity.HasOne(d => d.Tour).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_Comments_Tours");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Comments_Users");
        });

        modelBuilder.Entity<GuideTour>(entity =>
        {
            entity.HasKey(gt => new {gt.GuideId, gt.TourId});

            entity.HasOne(d => d.Guide).WithMany()
                .HasForeignKey(d => d.GuideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GuideTours_Users");

            entity.HasOne(d => d.Tour).WithMany()
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GuideTours_Tours");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Guide).WithMany(p => p.OrderGuides)
                .HasForeignKey(d => d.GuideId)
                .HasConstraintName("FK_Orders_Users1");

            entity.HasOne(d => d.Tour).WithMany(p => p.Orders)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_Orders_Tours");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.Property(e => e.Path).IsUnicode(false);

            entity.HasOne(d => d.Tour).WithMany(p => p.Photos)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_Photos_Tours");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("text");
            entity.Property(e => e.Name).HasColumnType("text");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Info).HasColumnType("text");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartPointGeo).HasColumnType("text");
            entity.Property(e => e.StartPointName).HasColumnType("text");

            entity.HasOne(d => d.Category).WithMany(p => p.Tours)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Tours_Categories");

            entity.HasOne(d => d.City).WithMany(p => p.Tours)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Tours_Cities");
                
            entity.Property(e => e.Name).HasColumnType("text");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Info).HasColumnType("text");
            entity.Property(e => e.ProfilePhoto).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
