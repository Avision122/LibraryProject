namespace Projekt_studia2
{
    using Microsoft.EntityFrameworkCore;
    using Projekt_studia2.Models;
    using System;

    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalQueue> RentalQueues { get; set; }
        public DbSet<ReturnQueue> ReturnQueues { get; set; }
        public DbSet<LogEvent> LogEvents { get; set; }
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("name=ConnectionStrings:DefaultConnection",
                new MySqlServerVersion(new Version(8, 0, 21)));
        }
    }

}
