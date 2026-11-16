using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Ważne!
using Microsoft.EntityFrameworkCore;
using CarRentalApp.Models;

namespace CarRentalApp.Data
{
    // Zmieniamy DbContext na IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}