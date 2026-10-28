using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniTruckDelivery.Models;


namespace MiniTruckDelivery.Date
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<TripExpense> TripExpenses => Set<TripExpense>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Driver>()
                .HasOne(driver => driver.User)
                .WithOne(user => user.DriverProfile)
                .HasForeignKey<Driver>(driver => driver.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Driver>()
                .Property(driver => driver.IsActive)
                .HasDefaultValue(true);

            builder.Entity<Trip>()
                .Property(trip => trip.IsCancelled)
                .HasDefaultValue(false);
        }
    }
}

