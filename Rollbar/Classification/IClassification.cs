namespace Rollbar.Classification
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface IClassification
    /// Implements the <see cref="Rollbar.Classification.Identifiable{TID}" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.Identifiable{TID}" />
    public interface IClassification
        : Identifiable<string>
    {
        /// <summary>
        /// Gets the classifiers count.
        /// </summary>
        /// <value>The classifiers count.</value>
        int ClassifiersCount { get; }
        /// <summary>
        /// Gets the classifiers.
        /// </summary>
        /// <value>The classifiers.</value>
        IEnumerable<IClassifier> Classifiers { get; }
        /// <summary>
        /// Gets the classifier types.
        /// </summary>
        /// <value>The classifier types.</value>
        IEnumerable<Type> ClassifierTypes { get; }
    }
}
