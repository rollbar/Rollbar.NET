namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;

    public class ValidationResult
    {
        protected static readonly ValidationResult[] emptyDetails = new ValidationResult[0];

        private readonly Enum _validationRule;
        public readonly IReadOnlyCollection<ValidationResult> _details;

        private ValidationResult()
        {

        }

        public ValidationResult(Enum validationRule)
            : this(validationRule, ValidationResult.emptyDetails)
        {

        }

        public ValidationResult(Enum validationRule, IEnumerable<ValidationResult> details)
        {
            this._validationRule = validationRule;
            this._details = new List<ValidationResult>(details);
        }

        public Enum ValidationRule
        {
            get { return this._validationRule; }
        }

        public IReadOnlyCollection<ValidationResult> Details
        {
            get { return this._details; }
        }

        public override string ToString()
        {
            return 
                $"{typeof(ValidationResult).Name}: "
                + $"failed rule {this.ValidationRule.GetType().Name}.{this.ValidationRule} "
                + $"with {this.Details.Count} details.";
        }
    }

    //public class ValidationResult<TValidationRule>
    //    : ValidationResult
    //    where TValidationRule : Enum
    //{
    //    public ValidationResult(TValidationRule validationRule) : base(validationRule)
    //    {
    //    }

    //    public ValidationResult(TValidationRule validationRule, IEnumerable<ValidationResult> details) 
    //        : base(validationRule, details)
    //    {
    //    }
    //}
}
