namespace Rollbar.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface IClassifier
    /// Implements the <see cref="Rollbar.Classification.Identifiable{System.String}" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.Identifiable{System.String}" />
    public interface IClassifier
        : Identifiable<string>
    {
        /// <summary>
        /// Gets the type of the classifier.
        /// </summary>
        /// <value>The type of the classifier.</value>
        Type ClassifierType { get; }
        /// <summary>
        /// Gets the classifier object.
        /// </summary>
        /// <value>The classifier object.</value>
        object ClassifierObject { get; }
    }
}
