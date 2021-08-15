namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class ValidationResult.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// The empty details
        /// </summary>
        protected static readonly ValidationResult[] emptyDetails = new ValidationResult[0];

        /// <summary>
        /// The validation rule
        /// </summary>
        private readonly Enum? _validationRule;
        /// <summary>
        /// The details
        /// </summary>
        public readonly IReadOnlyCollection<ValidationResult> resultDetails;

        /// <summary>
        /// Prevents a default instance of the <see cref="ValidationResult"/> class from being created.
        /// </summary>
        private ValidationResult()
        {
            this._validationRule = null;
            this.resultDetails = new List<ValidationResult>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="validationRule">The validation rule.</param>
        public ValidationResult(Enum validationRule)
            : this(validationRule, ValidationResult.emptyDetails)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="validationRule">The validation rule.</param>
        /// <param name="details">The details.</param>
        public ValidationResult(Enum validationRule, IEnumerable<ValidationResult> details)
        {
            this._validationRule = validationRule;
            this.resultDetails = new List<ValidationResult>(details);
        }

        /// <summary>
        /// Gets the validation rule.
        /// </summary>
        /// <value>The validation rule.</value>
        public Enum ValidationRule
        {
            get { return this._validationRule!; }
        }

        /// <summary>
        /// Gets the details.
        /// </summary>
        /// <value>The details.</value>
        public IReadOnlyCollection<ValidationResult> Details
        {
            get { return this.resultDetails; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return 
                $"{typeof(ValidationResult).Name}: "
                + $"failed rule {this.ValidationRule.GetType().Name}.{this.ValidationRule} "
                + $"with {this.Details.Count} details.";
        }
    }
}
