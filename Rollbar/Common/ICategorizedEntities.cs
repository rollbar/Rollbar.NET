namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Interface ICategorizedEntities
    /// </summary>
    /// <typeparam name="TCategory">The type of the t category.</typeparam>
    /// <typeparam name="TEntity">The type of the t entity.</typeparam>
    public interface ICategorizedEntities<TCategory, TEntity>
    {
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> Clear();

        /// <summary>
        /// Registers the category with its corresponding initial set of entities.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="entities">The entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> RegisterCategory(TCategory category, ISet<TEntity> entities);

        /// <summary>
        /// Registers the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> RegisterCategory(TCategory category);

        /// <summary>
        /// Unregisters category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> UnRegisterCategory(TCategory category);

        /// <summary>
        /// Expands the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="deltaEntities">The delta entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> ExpandCategory(TCategory category, ISet<TEntity> deltaEntities);

        /// <summary>
        /// Reduces the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="deltaEntities">The delta entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> ReduceCategory(TCategory category, ISet<TEntity> deltaEntities);

        /// <summary>
        /// Replaces the category's entities.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="entities">The entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        ICategorizedEntities<TCategory, TEntity> ReplaceCategory(TCategory category, ISet<TEntity> entities);

        /// <summary>
        /// Gets all the categories.
        /// </summary>
        /// <returns>ISet&lt;TCategory&gt;.</returns>
        ISet<TCategory> GetCategories();

        /// <summary>
        /// Gets the categories count.
        /// </summary>
        /// <returns>System.Int64.</returns>
        long GetCategoriesCount();

        /// <summary>
        /// Gets all the entities.
        /// </summary>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        ISet<TEntity> GetEntities();

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        ISet<TEntity> GetEntities(ISet<TCategory> categories);

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        ISet<TEntity> GetEntities(params TCategory[] categories);

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <returns>System.Int64.</returns>
        long GetEntitiesCount();

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>System.Int64.</returns>
        long GetEntitiesCount(ISet<TCategory> categories);

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>System.Int64.</returns>
        long GetEntitiesCount(params TCategory[] categories);
    }
}
