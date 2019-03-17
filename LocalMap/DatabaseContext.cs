using MapTest.Models;
using Microsoft.EntityFrameworkCore;

namespace MapTest
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<TileModel> Tiles { get; set; }

        public DbSet<DataModel> Images { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=2017-07-03_planet_z0_z14.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TileModel>().HasKey(model => new
            {
                model.ZoomLevel, model.Column, model.Row
            });

            modelBuilder.Entity<DataModel>().HasKey(model => model.TileId);

            modelBuilder.Entity<TileModel>().HasOne(tileModel => tileModel.Data);
        }
    }
}
