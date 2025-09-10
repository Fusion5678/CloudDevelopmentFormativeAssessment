using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VenueDBApp.Data;

public partial class VenueDbContext : DbContext
{
    public VenueDbContext()
    {
    }

    public VenueDbContext(DbContextOptions<VenueDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingSummary> BookingSummaries { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951ACD9C703978");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.VenueId).HasColumnName("VenueID");

            entity.HasOne(d => d.Event).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__EventID__628FA481");

            entity.HasOne(d => d.Venue).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Booking__VenueID__6383C8BA");
        });

        modelBuilder.Entity<BookingSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BookingSummary");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.EventId).HasColumnName("eventID");
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.VenueId).HasColumnName("venueID");
            entity.Property(e => e.VenueName).HasMaxLength(100);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__7944C870E480FCE8");

            entity.ToTable("Event");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.VenueId).HasColumnName("VenueID");

            entity.HasOne(d => d.Venue).WithMany(p => p.Events)
                .HasForeignKey(d => d.VenueId)
                .HasConstraintName("FK__Event__VenueID__6477ECF3");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("PK__Venue__3C57E5D2768E8A66");

            entity.ToTable("Venue");

            entity.Property(e => e.VenueId).HasColumnName("VenueID");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.VenueName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
