using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GreenMobility.Models;

public partial class GreenMobilityContext : DbContext
{
    public GreenMobilityContext()
    {
    }

    public GreenMobilityContext(DbContextOptions<GreenMobilityContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bicycle> Bicycles { get; set; }

    public virtual DbSet<BicycleStatus> BicycleStatuses { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Parking> Parkings { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<RentalDetail> RentalDetails { get; set; }

    public virtual DbSet<RentalStatus> RentalStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-KV708U9\\SQLEXPRESS;Database=GreenMobility;User Id=sa;Password=1;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bicycle>(entity =>
        {
            entity.Property(e => e.BicycleId).HasColumnName("BicycleID");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.BicycleName).HasMaxLength(50);
            entity.Property(e => e.BicycleStatusId).HasColumnName("BicycleStatusID");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DateModified).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.LicensePlate).HasMaxLength(50);
            entity.Property(e => e.ParkingId).HasColumnName("ParkingID");
            entity.Property(e => e.Photo).HasMaxLength(255);

            entity.HasOne(d => d.BicycleStatus).WithMany(p => p.Bicycles)
                .HasForeignKey(d => d.BicycleStatusId)
                .HasConstraintName("FK_Bicycles_BicycleStatus");

            entity.HasOne(d => d.Parking).WithMany(p => p.Bicycles)
                .HasForeignKey(d => d.ParkingId)
                .HasConstraintName("FK_Bicycles_Parkings");
        });

        modelBuilder.Entity<BicycleStatus>(entity =>
        {
            entity.ToTable("BicycleStatus");

            entity.Property(e => e.BicycleStatusId)
                .ValueGeneratedNever()
                .HasColumnName("BicycleStatusID");
            entity.Property(e => e.Description).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Salt)
                .HasMaxLength(8)
                .IsFixedLength()
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.BirthDate).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.DateModified).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ParkingId).HasColumnName("ParkingID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Photo).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Parking).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ParkingId)
                .HasConstraintName("FK_Employees_Parkings");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Roles");
        });

        modelBuilder.Entity<Parking>(entity =>
        {
            entity.Property(e => e.ParkingId).HasColumnName("ParkingID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Alias).HasMaxLength(50);
            entity.Property(e => e.ParkingName).HasMaxLength(50);
            entity.Property(e => e.Photo).HasMaxLength(255);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Alias)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Contents).UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ShortContents).HasMaxLength(255);
            entity.Property(e => e.Thumb)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.Property(e => e.RentalId).HasColumnName("RentalID");
            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("CustomerID");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.OrderTime)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.PickupEmployeeId).HasColumnName("PickupEmployeeID");
            entity.Property(e => e.PickupTime).HasColumnType("datetime");
            entity.Property(e => e.RentalStatusId).HasColumnName("RentalStatusID");
            entity.Property(e => e.ReturnEmployeeId).HasColumnName("ReturnEmployeeID");
            entity.Property(e => e.ReturnTime).HasColumnType("datetime");
            entity.Property(e => e.Surcharge).HasDefaultValue(0.0);

            entity.HasOne(d => d.Customer).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Rentals_Customers");

            entity.HasOne(d => d.PickupEmployee).WithMany(p => p.RentalPickupEmployees)
                .HasForeignKey(d => d.PickupEmployeeId)
                .HasConstraintName("FK_Rentals_Employees1");

            entity.HasOne(d => d.PickupParkingNavigation).WithMany(p => p.RentalPickupParkingNavigations)
                .HasForeignKey(d => d.PickupParking)
                .HasConstraintName("FK_Rentals_Parkings");

            entity.HasOne(d => d.RentalStatus).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.RentalStatusId)
                .HasConstraintName("FK_Rentals_RentalStatus");

            entity.HasOne(d => d.ReturnEmployee).WithMany(p => p.RentalReturnEmployees)
                .HasForeignKey(d => d.ReturnEmployeeId)
                .HasConstraintName("FK_Rentals_Employees2");

            entity.HasOne(d => d.ReturnParkingNavigation).WithMany(p => p.RentalReturnParkingNavigations)
                .HasForeignKey(d => d.ReturnParking)
                .HasConstraintName("FK_Rentals_Parkings1");
        });

        modelBuilder.Entity<RentalDetail>(entity =>
        {
            entity.Property(e => e.RentalDetailId).HasColumnName("RentalDetailID");
            entity.Property(e => e.AppointmentTime).HasColumnType("datetime");
            entity.Property(e => e.BicycleId).HasColumnName("BicycleID");
            entity.Property(e => e.RentalId).HasColumnName("RentalID");

            entity.HasOne(d => d.Bicycle).WithMany(p => p.RentalDetails)
                .HasForeignKey(d => d.BicycleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RentalDetails_Bicycles");

            entity.HasOne(d => d.Rental).WithMany(p => p.RentalDetails)
                .HasForeignKey(d => d.RentalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RentalDetails_Rentals");
        });

        modelBuilder.Entity<RentalStatus>(entity =>
        {
            entity.ToTable("RentalStatus");

            entity.Property(e => e.RentalStatusId)
                .ValueGeneratedNever()
                .HasColumnName("RentalStatusID");
            entity.Property(e => e.Description).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
