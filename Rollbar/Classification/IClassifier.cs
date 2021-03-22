namespace Rollbar.Classification
{
    using System;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Interface IClassifier
    /// Implements the <see cref="Rollbar.Classification.Identifiable{System.String}" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.Identifiable{System.String}" />
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
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
