namespace Rollbar.PayloadStore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Class StoreContext.
    /// Implements the <see cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class StoreContext
        : DbContext
    {
        static StoreContext()
        {
#if (!NETFX || NETFX_461nNewer)
            SQLitePCL.Batteries_V2.Init();
#endif
        }

        /// <summary>
        /// The default rollbar store database file location
        /// </summary>
        public static readonly string DefaultRollbarStoreDbFileLocation = PayloadStoreConstants.DefaultRollbarStoreDbFileLocation;

        /// <summary>
        /// Gets or sets the full name of the rollbar store database.
        /// </summary>
        /// <value>The full name of the rollbar store database.</value>
        public static string RollbarStoreDbFullName { get; set; } = PayloadStoreConstants.DefaultRollbarStoreDbFile;

        /// <summary>
        /// The sqlite connection string
        /// </summary>
        private static string sqliteConnectionString { get; set; } = $"Filename={StoreContext.RollbarStoreDbFullName};";

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

        /// <summary>
        /// Makes the sure database exists and ready.
        /// </summary>
        public void MakeSureDatabaseExistsAndReady()
        {
            //Type[] migrations =
            //    ReflectionUtility.GetSubClassesOf(typeof(Migration), this.GetType().Assembly);

            //if (migrations != null && migrations.LongLength > 0)
            //{
            //    // when using migrations:
            //    this.Database.Migrate(); // if needed creates db and runs migrations
            //}
            //else
            {
                // when not using migrations:
                this.Database.EnsureCreated(); // if needed creates db (but doesn't run migrations)
            }
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
