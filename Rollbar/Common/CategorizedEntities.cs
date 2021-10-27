namespace Rollbar.Common
{
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Class CategorizedEntities.
    /// Implements the <see cref="Rollbar.Common.ICategorizedEntities{TCategory, TEntity}" />
    /// </summary>
    /// <typeparam name="TCategory">The type of the t category.</typeparam>
    /// <typeparam name="TEntity">The type of the t entity.</typeparam>
    /// <seealso cref="Rollbar.Common.ICategorizedEntities{TCategory, TEntity}" />
    public class CategorizedEntities<TCategory, TEntity>
        : ICategorizedEntities<TCategory, TEntity>
        where TCategory : notnull
    {
        /// <summary>
        /// The entities by category synchronize root
        /// </summary>
        private readonly object _entitiesByCategorySyncRoot =
            new object();

        /// <summary>
        /// The entities by category
        /// </summary>
        private readonly Dictionary<TCategory, HashSet<TEntity>> _entitiesByCategory =
            new();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> Clear()
        {
            IEnumerable<HashSet<TEntity>> entitySets;
            lock (_entitiesByCategorySyncRoot)
            {
                entitySets = _entitiesByCategory.Values.ToArray();
                _entitiesByCategory.Clear();
            }

            foreach (var entitySet in entitySets)
            {
                entitySet.Clear();
            }

            return this;
        }

        /// <summary>
        /// Registers the category with its corresponding initial set of entities.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="entities">The entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> RegisterCategory(
            TCategory category, 
            ISet<TEntity> entities
            )
        {
            HashSet<TEntity> entitiesSet = new HashSet<TEntity>(entities);

            lock (_entitiesByCategorySyncRoot)
            {
                _entitiesByCategory.Add(category, entitiesSet);
            }

            return this;
        }

        /// <summary>
        /// Registers the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> RegisterCategory(TCategory category)
        {
            return this.RegisterCategory(category, new HashSet<TEntity>());
        }

        /// <summary>
        /// Unregisters category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> UnRegisterCategory(TCategory category)
        {
            lock (_entitiesByCategorySyncRoot)
            {
                _entitiesByCategory.Remove(category);
            }

            return this;
        }

        /// <summary>
        /// Expands the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="deltaEntities">The delta entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> ExpandCategory(
            TCategory category, 
            ISet<TEntity> deltaEntities
            )
        {
            lock (_entitiesByCategorySyncRoot)
            {
                _entitiesByCategory[category].UnionWith(deltaEntities);
            }

            return this;
        }

        /// <summary>
        /// Reduces the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="deltaEntities">The delta entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> ReduceCategory(
            TCategory category, 
            ISet<TEntity> deltaEntities
            )
        {
            lock (_entitiesByCategorySyncRoot)
            {
                _entitiesByCategory[category].ExceptWith(deltaEntities);
            }

            return this;
        }

        /// <summary>
        /// Replaces the category's entities.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="entities">The entities.</param>
        /// <returns>ICategorizedEntities&lt;TCategory, TEntity&gt;.</returns>
        public ICategorizedEntities<TCategory, TEntity> ReplaceCategory(
            TCategory category, 
            ISet<TEntity> entities
            )
        {
            HashSet<TEntity> entitiesSet = new HashSet<TEntity>(entities);

            lock (_entitiesByCategorySyncRoot)
            {
                _entitiesByCategory[category] = entitiesSet;
            }

            return this;
        }

        /// <summary>
        /// Gets the categories count.
        /// </summary>
        /// <returns>System.Int64.</returns>
        public long GetCategoriesCount()
        {
            long count = 0;

            lock (_entitiesByCategorySyncRoot)
            {
                count = _entitiesByCategory.Keys.Count;
            }

            return count;
        }

        /// <summary>
        /// Gets all the entities.
        /// </summary>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        public ISet<TEntity> GetEntities()
        {
            lock (_entitiesByCategorySyncRoot)
            {
                return MergeSets(_entitiesByCategory.Values);
            }
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        public ISet<TEntity> GetEntities(ISet<TCategory> categories)
        {
            lock (_entitiesByCategorySyncRoot)
            {
                var resultingSets = _entitiesByCategory
                    .Where(kvp => categories.Contains(kvp.Key))
                    .Select(kvp => kvp.Value)
                    .ToList();

                return MergeSets(resultingSets);
            }
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>ISet&lt;TEntity&gt;.</returns>
        public ISet<TEntity> GetEntities(params TCategory[] categories)
        {
            HashSet<TCategory> categoriesSet = new HashSet<TCategory>(categories);
            return this.GetEntities(categoriesSet);
        }

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <returns>System.Int64.</returns>
        public long GetEntitiesCount()
        {
            long count = 0;

            lock (_entitiesByCategorySyncRoot)
            {
                count = _entitiesByCategory
                    .Values
                    .Aggregate(count, func: (total, next) => total + next.Count)
                    ;
            }

            return count;
        }

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>System.Int64.</returns>
        public long GetEntitiesCount(ISet<TCategory> categories)
        {
            long count = 0;

            lock (_entitiesByCategorySyncRoot)
            {
                count = _entitiesByCategory
                    .Where(kvp => categories.Contains(kvp.Key))
                    .Select(kvp => kvp.Value)
                    .Aggregate(count, (total, next) => total + next.Count)
                    ;
            }

            return count;
        }

        /// <summary>
        /// Gets the entities count.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>System.Int64.</returns>
        public long GetEntitiesCount(params TCategory[] categories)
        {
            HashSet<TCategory> categoriesSet = new HashSet<TCategory>(categories);
            return this.GetEntitiesCount(categoriesSet);
        }

        /// <summary>
        /// Gets all the categories.
        /// </summary>
        /// <returns>ISet&lt;TCategory&gt;.</returns>
        public ISet<TCategory> GetCategories()
        {
            lock (_entitiesByCategorySyncRoot)
            {
                return new HashSet<TCategory>(_entitiesByCategory.Keys);
            }
        }

        /// <summary>
        /// Merges the sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setsToMerge">The sets to merge.</param>
        /// <returns>ISet&lt;T&gt;.</returns>
        private ISet<T> MergeSets<T>(IEnumerable<ISet<T>> setsToMerge)
        {
            HashSet<T> resultingSet = new HashSet<T>();

            foreach (var set in setsToMerge)
            {
                resultingSet.UnionWith(set);
            }

            return resultingSet;
        }

    }
}
