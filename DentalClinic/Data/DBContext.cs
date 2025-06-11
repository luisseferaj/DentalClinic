using Microsoft.EntityFrameworkCore;
using DentalClinic.Data.Models;

namespace DentalClinic.Data
{
    public class DBContext: DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public DbSet<Models.Users> Users { get; set; }
        public DbSet<Models.Hosts> Doctor { get; set; }
        public DbSet<Models.Client> Client { get; set; }
        public DbSet<Models.Services> Services { get; set; }
        public DbSet<Models.Favorites> Favorites { get; set; }
        public DbSet<Models.Bookings> Bookings { get; set; }
        public DbSet<Models.Reportings> Reportings { get; set; }
        public DbSet<Models.Reviews> Reviews { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bookings>()
                .HasKey(b => new { b.BookingTime, b.Service_Id, b.Date});

            modelBuilder.Entity<Reportings>()
               .HasKey(b => new { b.BookingTime, b.Service_Id, b.Date });

            modelBuilder.Entity<Favorites>()
               .HasKey(b => new { b.Client_Id, b.Service_Id});
        }
    }
}
