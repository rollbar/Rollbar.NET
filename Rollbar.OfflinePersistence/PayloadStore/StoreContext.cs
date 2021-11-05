namespace Rollbar.PayloadStore
{
    using System;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Migrations;

    using Rollbar;
    using Rollbar.Common;

    /// <summary>
    /// Class StoreContext.
    /// Implements the <see cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    internal class StoreContext
        : DbContext
    {
        static StoreContext()
        {
            SQLitePCL.Batteries_V2.Init();
        }

        /// <summary>
        /// The default rollbar store database file location
        /// </summary>
        public static readonly string? DefaultRollbarStoreDbFileLocation = 
            PayloadStoreConstants.DefaultRollbarStoreDbFileLocation;

        /// <summary>
        /// Gets or sets the full name of the rollbar store database.
        /// </summary>
        /// <value>The full name of the rollbar store database.</value>
        public static string RollbarStoreDbFullName { get; set; } = 
            PayloadStoreConstants.DefaultRollbarStoreDbFile;

        /// <summary>
        /// The sqlite connection string
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        private static string sqliteConnectionString { get; set; } =
#pragma warning restore IDE1006 // Naming Styles
            $"Filename={StoreContext.RollbarStoreDbFullName};";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the destinations.
        /// </summary>
        /// <value>The destinations.</value>
        public DbSet<Destination> Destinations { get; set; }

        /// <summary>
        /// Gets or sets the payload records.
        /// </summary>
        /// <value>The payload records.</value>
        public DbSet<PayloadRecord> PayloadRecords { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Makes the sure database exists and ready.
        /// </summary>
        public void MakeSureDatabaseExistsAndReady()
        {
            // In this implementation we are trying to be as robust as possible.
            bool success;

            // 1. Try to migrate-create gracefully (this is the best case scenario):
            try
            {
                this.Database.Migrate(); // if needed creates db and runs migrations
                success = true;
            }
            catch (Exception ex)
            {
                RollbarErrorUtility.Report(
                    null,
                    null,
                    InternalRollbarError.PersistentStoreContextError,
                    "Filed initial database migration-creation call!",
                    ex,
                    null);
                success = false;
            }

            if (success)
            {
                return;
            }

            // 2. Try to migrate-create gracefully after deleting the existing database
            //    (yes, we may lose some of the data but we may have a chance to become operational again):
            this.Database.EnsureDeleted();
            try
            {
                this.Database.Migrate(); // if needed creates db and runs migrations
                success = true;
            }
            catch (Exception ex)
            {
                RollbarErrorUtility.Report(
                    null,
                    null,
                    InternalRollbarError.PersistentStoreContextError,
                    "Filed database secondary migration-creation call!",
                    ex,
                    null);
                success = false;
            }

            if (success)
            {
                return;
            }

            // 3. Try to create a new database (without migrations support):
            try
            {
                this.Database.EnsureCreated(); // if needed creates db (but doesn't run migrations)
                success = true;
            }
            catch (Exception ex)
            {
                RollbarErrorUtility.Report(
                    null,
                    null,
                    InternalRollbarError.PersistentStoreContextError,
                    "Filed database creation (without migrations support)!",
                    ex,
                    null);
                success = false;
            }

            if (success)
            {
                return;
            }

            // 4. Now, giving up:
            RollbarErrorUtility.Report(
                null,
                null,
                InternalRollbarError.PersistentStoreContextError,
                "Filed all attempts to create the database!",
                null,
                null);
        }

        #region Overrides of DbContext

        /// <summary>
        /// <para>
        /// Override this method to configure the database (and other options) to be used for this context.
        /// This method is called for each instance of the context that is created.
        /// The base implementation does nothing.
        /// </para>
        /// <para>
        /// In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions" /> may or may not have been passed
        /// to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured" /> to determine if
        /// the options have already been set, and skip some or all of the logic in
        /// <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />.
        /// </para>
        /// </summary>
        /// <param name="optionsBuilder">A builder used to create or modify options for this context. Databases (and other extensions)
        /// typically define extension methods on this object that allow you to configure the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            StoreContext.sqliteConnectionString = $"Filename={StoreContext.RollbarStoreDbFullName};";
            optionsBuilder.UseSqlite(StoreContext.sqliteConnectionString);
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.</remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Destination>()
                .ToTable("Destinations")
                .HasIndex(d => new { d.Endpoint, d.AccessToken })
                ;
            modelBuilder.Entity<Destination>()
                .HasKey(d => d.ID)
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

            base.OnModelCreating(modelBuilder);
        }

        #endregion

    }
}
