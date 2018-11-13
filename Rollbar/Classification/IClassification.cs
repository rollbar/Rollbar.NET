namespace Rollbar.Classification
{
    using System;
    using System.Collections.Generic;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute    
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Interface IClassification
    /// Implements the <see cref="Rollbar.Classification.Identifiable{System.String}" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.Identifiable{System.String}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
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
