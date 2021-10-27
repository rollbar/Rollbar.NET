namespace Rollbar.Classification
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Class Classifier.
    /// Implements the <see cref="Rollbar.Classification.IClassifier" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.IClassifier" />
    public class Classifier
        : IClassifier
    {
        /// <summary>
        /// The classifiers by identifier
        /// </summary>
        private static readonly ConcurrentDictionary<string, Classifier> classifiersByID = 
            new ConcurrentDictionary<string, Classifier>();

        /// <summary>
        /// Matches the classifier.
        /// </summary>
        /// <param name="classifierObject">The classifier object.</param>
        /// <returns>Classifier.</returns>
        public static Classifier MatchClassifier(object classifierObject)
        {
            Classifier classifier = new Classifier(classifierObject);
            classifier = Classifier.classifiersByID.GetOrAdd(classifier.ID, classifier);
            return classifier;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Classifier"/> class from being created.
        /// </summary>
        private Classifier()
            : this(new object())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Classifier"/> class.
        /// </summary>
        /// <param name="classifierObject">The classifier object.</param>
        protected Classifier(object classifierObject)
        {
            this.ClassifierObject = classifierObject;
            this.ClassifierType = classifierObject.GetType();
            this.ID = GenerateClassifierID(classifierObject);
        }

        /// <summary>
        /// Gets the classifier object.
        /// </summary>
        /// <value>The classifier object.</value>
        public object ClassifierObject { get; private set; }

        /// <summary>
        /// Gets the type of the classifier.
        /// </summary>
        /// <value>The type of the classifier.</value>
        public Type ClassifierType { get; private set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID { get; private set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        object Identifiable.ID { get { return this.ID; } }

        /// <summary>
        /// Gets all known IDs.
        /// </summary>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        /// <value>All known IDs.</value>
        public IEnumerable<string> GetAllKnownIDs() { return Classifier.classifiersByID.Keys; }

        /// <summary>
        /// Gets all known IDs.
        /// </summary>
        /// <returns>IEnumerable&lt;System.Object&gt;.</returns>
        /// <value>All known IDs.</value>
        IEnumerable<object> Identifiable.GetAllKnownIDs() { return this.GetAllKnownIDs(); }

        /// <summary>
        /// Generates the classifier identifier.
        /// </summary>
        /// <param name="classifierObject">The classifier object.</param>
        /// <returns>System.String.</returns>
        /// <remarks>
        /// WARNING:
        /// This method is called from the type constructor.
        /// Make sure you know what you are doing when overriding it.
        /// </remarks>
        protected string GenerateClassifierID(object classifierObject)
        {
            return classifierObject.GetType().FullName + ": " + classifierObject;
        }
    }
}
