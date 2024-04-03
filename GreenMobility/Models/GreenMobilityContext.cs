using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GreenMobility.Models
{
    public partial class GreenMobilityContext : DbContext
    {
        public GreenMobilityContext()
        {
        }

        public GreenMobilityContext(DbContextOptions<GreenMobilityContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bicycle> Bicycles { get; set; } = null!;
        public virtual DbSet<BicycleStatus> BicycleStatuses { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<Parking> Parkings { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<Rental> Rentals { get; set; } = null!;
        public virtual DbSet<RentalDetail> RentalDetails { get; set; } = null!;
        public virtual DbSet<RentalStatus> RentalStatuses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DESKTOP-2BDMKVT\\SQLEXPRESS1;Database=GreenMobility;User Id=sa;Password=1;TrustServerCertificate=True");
            }
        }

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

                entity.HasOne(d => d.BicycleStatus)
                    .WithMany(p => p.Bicycles)
                    .HasForeignKey(d => d.BicycleStatusId)
                    .HasConstraintName("FK_Bicycles_BicycleStatus");

                entity.HasOne(d => d.Parking)
                    .WithMany(p => p.Bicycles)
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

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(255);

                entity.Property(e => e.Salt)
                    .HasMaxLength(8)
                    .IsFixedLength()
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.ParkingId).HasColumnName("ParkingID");

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(255);

                entity.Property(e => e.Photo).HasMaxLength(255);

                entity.HasOne(d => d.Parking)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.ParkingId)
                    .HasConstraintName("FK_Employees_Parkings");
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

                entity.Property(e => e.Author)
                    .HasMaxLength(255)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CatId).HasColumnName("CatID");

                entity.Property(e => e.Contents).UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IsHot).HasColumnName("isHot");

                entity.Property(e => e.IsNewfeed).HasColumnName("isNewfeed");

                entity.Property(e => e.MetaDesc)
                    .HasMaxLength(255)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.MetaKey)
                    .HasMaxLength(255)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Scontents)
                    .HasMaxLength(255)
                    .HasColumnName("SContents")
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Tags).UseCollation("SQL_Latin1_General_CP1_CI_AS");

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

                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.OrderTime).HasColumnType("datetime");

                entity.Property(e => e.PickupTime).HasColumnType("datetime");

                entity.Property(e => e.RentalStatusId).HasColumnName("RentalStatusID");

                entity.Property(e => e.Surcharge).HasColumnType("money");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Rentals)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Rentals_Customers");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Rentals)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_Rentals_Employees");

                entity.HasOne(d => d.PickupParkingNavigation)
                    .WithMany(p => p.RentalPickupParkingNavigations)
                    .HasForeignKey(d => d.PickupParking)
                    .HasConstraintName("FK_Rentals_Parkings");

                entity.HasOne(d => d.RentalStatus)
                    .WithMany(p => p.Rentals)
                    .HasForeignKey(d => d.RentalStatusId)
                    .HasConstraintName("FK_Rentals_RentalStatus");

                entity.HasOne(d => d.ReturnParkingNavigation)
                    .WithMany(p => p.RentalReturnParkingNavigations)
                    .HasForeignKey(d => d.ReturnParking)
                    .HasConstraintName("FK_Rentals_Parkings1");
            });

            modelBuilder.Entity<RentalDetail>(entity =>
            {
                entity.Property(e => e.RentalDetailId).HasColumnName("RentalDetailID");

                entity.Property(e => e.BicycleId).HasColumnName("BicycleID");

                entity.Property(e => e.PickupTime).HasColumnType("datetime");

                entity.Property(e => e.RentalId).HasColumnName("RentalID");

                entity.HasOne(d => d.Bicycle)
                    .WithMany(p => p.RentalDetails)
                    .HasForeignKey(d => d.BicycleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RentalDetails_Bicycles");

                entity.HasOne(d => d.Rental)
                    .WithMany(p => p.RentalDetails)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RentalDetails_Rentals");
            });

            modelBuilder.Entity<RentalStatus>(entity =>
            {
                entity.ToTable("RentalStatus");

                entity.Property(e => e.RentalStatusId).HasColumnName("RentalStatusID");

                entity.Property(e => e.Description).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
