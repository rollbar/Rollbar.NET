namespace Rollbar.PayloadStore {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Migrations;

    public class StoreContext 
    : DbContext
    {
        private const string sqliteConnectionString = @"Data Source=RollbarPayloadsStore.db;";

        public DbSet<Destination> Destinations { get; set; }
        public DbSet<PayloadRecord> PayloadRecords { get; set; }

        public void MakeSureDatabaseExistsAndReady()
        {
            Type[] migrations = 
                ReflectionUtility.GetSubClassesOf(typeof(Migration), this.GetType().Assembly);

            if(migrations != null && migrations.LongLength > 0) 
            {
                // when using migrations:
                this.Database.Migrate(); // if needed creates db and runs migrations
            }
            else
            {
                // when not using migrations:
                this.Database.EnsureCreated(); // if needed creates db (but doesn't run migrations)
            }
        }

        #region Overrides of DbContext

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(sqliteConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Destination>()
                .ToTable("Destinations")
                .HasIndex(d => new {d.Endpoint, d.AccessToken})
                ;
            modelBuilder.Entity<Destination>()
                .HasKey(d=>d.ID)
                ;
            modelBuilder.Entity<Destination>()
                .Property(d => d.Endpoint)
                .IsRequired(true);
            modelBuilder.Entity<Destination>()
                .Property(d => d.AccessToken)
                .IsRequired(true);

            modelBuilder.Entity<PayloadRecord>()
                .ToTable("PayloadRecords")
                .HasIndex(pr => pr.DestinationID)
                ;
            modelBuilder.Entity<PayloadRecord>()
                .HasKey(pr => pr.ID)
                ;
            modelBuilder.Entity<PayloadRecord>()
                .HasOne<Destination>(pr => pr.Destination)
                .WithMany(d => d.PayloadRecords)
                .HasForeignKey(pr => pr.DestinationID)
                .IsRequired(true)
                ;
            modelBuilder.Entity<PayloadRecord>()
                .Property(pr => pr.PayloadJson)
                .IsRequired(true)
                ;
            modelBuilder.Entity<PayloadRecord>()
                .Property(pr => pr.Timestamp)
                .IsRequired(true)
                ;
        }

        #endregion

    }
}
