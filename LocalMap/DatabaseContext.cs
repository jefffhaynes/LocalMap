﻿using LocalMap.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalMap
{
    internal class DatabaseContext : DbContext
    {
        public DbSet<TileModel> Tiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=2017-07-03_planet_z0_z14.mbtiles");
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
