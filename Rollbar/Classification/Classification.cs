namespace Rollbar.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using Rollbar.Diagnostics;
    using System.Collections.Concurrent;

    /// <summary>
    /// Class Classification.
    /// Implements the <see cref="Rollbar.Classification.IClassification" />
    /// </summary>
    /// <seealso cref="Rollbar.Classification.IClassification" />
    public class Classification
        : IClassification
    {
        /// <summary>
        /// The classifications by identifier
        /// </summary>
        private static readonly ConcurrentDictionary<string, Classification> classificationsByID = 
            new ConcurrentDictionary<string, Classification>();

        /// <summary>
        /// Matches the classification.
        /// </summary>
        /// <param name="classifierObjects">The classifier objects.</param>
        /// <returns>Classification.</returns>
        public static Classification MatchClassification(params object[] classifierObjects)
        {
            return Classification.MatchClassification(classifierObjects.AsEnumerable());
        }

        /// <summary>
        /// Matches the classification.
        /// </summary>
        /// <param name="classifierObjects">The classifier objects.</param>
        /// <returns>Classification.</returns>
        public static Classification MatchClassification(IEnumerable<object> classifierObjects)
        {
            List<Classifier> classifiers = new List<Classifier>(classifierObjects.Count());
            foreach(var item in classifierObjects)
            {
                classifiers.Add(Classifier.MatchClassifier(item));
            }
            return Classification.MatchClassification(classifiers);
        }

        /// <summary>
        /// Matches the classification.
        /// </summary>
        /// <param name="classifiers">The classifiers.</param>
        /// <returns>Classification.</returns>
        public static Classification MatchClassification(params Classifier[] classifiers)
        {
            return Classification.MatchClassification(classifiers.AsEnumerable());
        }

        /// <summary>
        /// Matches the classification.
        /// </summary>
        /// <param name="classifiers">The classifiers.</param>
        /// <returns>Classification.</returns>
        public static Classification MatchClassification(IEnumerable<Classifier> classifiers)
        {
            Classification classification = new Classification(classifiers);
            classification = Classification.classificationsByID.GetOrAdd(classification.ID, classification);
            return classification;
        }

        /// <summary>
        /// The classifiers
        /// </summary>
        private readonly ICollection<Classifier> _classifiers;
        /// <summary>
        /// The classifier types
        /// </summary>
        private readonly ICollection<Type> _classifierTypes;

        /// <summary>
        /// Prevents a default instance of the <see cref="Classification"/> class from being created.
        /// </summary>
        private Classification()
        {
            this._classifiers = new HashSet<Classifier>();
            this._classifierTypes = new HashSet<Type>();
            this.ID = this.GenerateID(this._classifiers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Classification"/> class.
        /// </summary>
        /// <param name="classifiers">The classifiers.</param>
        protected Classification(params Classifier[] classifiers)
            : this(classifiers.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Classification"/> class.
        /// </summary>
        /// <param name="classifiers">The classifiers.</param>
        protected Classification(IEnumerable<Classifier> classifiers)
        {
            if (classifiers == null)
            {
                this._classifiers = new HashSet<Classifier>();
                this._classifierTypes = new HashSet<Type>();
            }
            else
            {
                Assumption.AssertTrue(classifiers.Count() == classifiers.Distinct().Count(), nameof(classifiers));

                this._classifiers = new HashSet<Classifier>(classifiers);

                var classifierTypes = this._classifiers.Select(i => i.ClassifierObject.GetType()).ToList();
                this._classifierTypes = new HashSet<Type>(classifierTypes);

                Assumption.AssertEqual(this._classifierTypes.Count, this._classifiers.Count, nameof(this._classifierTypes.Count));
            }

            this.ID = this.GenerateID(this._classifiers);
        }

        /// <summary>
        /// Generates the identifier.
        /// </summary>
        /// <param name="classifiers">The classifiers.</param>
        /// <returns>System.String.</returns>
        /// <remarks>
        /// WARNING:
        /// This method is called from the type constructor.
        /// Make sure you know what you are doing when overriding it.
        /// </remarks>
        protected string GenerateID(IEnumerable<Classifier> classifiers)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var classifier in classifiers)
            {
                if (sb.Length == 0)
                {
                    sb.Append(classifier.ID);
                }
                else
                {
                    sb.Append(" + " + classifier.ID);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the classifiers count.
        /// </summary>
        /// <value>The classifiers count.</value>
        public int ClassifiersCount { get { return this._classifiers.Count; } }

        /// <summary>
        /// Gets the classifiers.
        /// </summary>
        /// <value>The classifiers.</value>
        public IEnumerable<IClassifier> Classifiers { get { return this._classifiers; } }

        /// <summary>
        /// Gets the classifier types.
        /// </summary>
        /// <value>The classifier types.</value>
        public IEnumerable<Type> ClassifierTypes { get { return this._classifierTypes; } }

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
        public IEnumerable<string> GetAllKnownIDs()
        {
            return Classification.classificationsByID.Keys;
        }

        /// <summary>
        /// Gets all known IDs.
        /// </summary>
        /// <returns>IEnumerable&lt;System.Object&gt;.</returns>
        /// <value>All known IDs.</value>
        IEnumerable<object> Identifiable.GetAllKnownIDs()
        {
            return this.GetAllKnownIDs();
        }
    }
}
