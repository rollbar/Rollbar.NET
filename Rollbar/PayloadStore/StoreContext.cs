namespace Rollbar.PayloadStore {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore;

    public class StoreContext 
    : DbContext
    {
        private const string sqliteConnectionString = @"Data Source=RollbarPayloadsStore.db;Version=3;UseUTF8Encoding=True;";
        public DbSet<Destination> Destinations { get; }
        public DbSet<PayloadRecord> PayloadRecords { get; }

        #region Overrides of DbContext

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlite(sqliteConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Destination>().ToTable("Destinations");
            modelBuilder.Entity<PayloadRecord>().ToTable("PayloadRecords");
        }

        #endregion
    }
}
