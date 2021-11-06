namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    using Rollbar.Infrastructure;

    /// <summary>
    /// Class Validator.
    /// </summary>
    public abstract class Validator
    {
        /// <summary>
        /// Gets or sets the default validation rules capacity.
        /// </summary>
        /// <value>The default validation rules capacity.</value>
        protected static int DefaultValidationRulesCapacity { get; set; } = 10;

        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public abstract IReadOnlyCollection<ValidationResult> Validate(object? validationSubject);

        /// <summary>
        /// Enum ValidationRule
        /// </summary>
        public enum ValidationRule
        {
            /// <summary>
            /// The validation subject instance required
            /// </summary>
            [Description("A validation subject can not be a null-reference.")]
            ValidationSubjectInstanceRequired,

            /// <summary>
            /// The match validation subject type
            /// </summary>
            [Description("A validation subject must match the expected type to be validated as such.")]
            MatchValidationSubjectType,
        }

        /// <summary>
        /// Validates the specified validatable.
        /// </summary>
        /// <param name="validatable">The validatable.</param>
        /// <param name="errorTag">The error tag.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void Validate(IValidatable? validatable, InternalRollbarError errorTag, string? errorMessage)
        {
            if(validatable != null)
            {
                var result = validatable.Validate();
                if(result != null && result.Count > 0)
                {
                    if(string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = $"{errorTag}.";
                    }
                    errorMessage = $"{errorMessage}. See Data property of this exception for validation details.";
                    RollbarException exception = new(errorTag, errorMessage);
                    exception.Data[errorTag] = result;
                    throw exception;
                }
            }
        }
    }

    /// <summary>
    /// Class CompositeValidator.
    /// Implements the <see cref="Rollbar.Common.Validator" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.Validator" />
    public class CompositeValidator
        : Validator
    {
        private readonly List<IValidatable> _validatables;
        private readonly List<Validator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeValidator"/> class.
        /// </summary>
        /// <param name="validators">The validators.</param>
        /// <param name="validatables">The validatables.</param>
        public CompositeValidator(IEnumerable<Validator> validators, IEnumerable<IValidatable>? validatables = null)
        {
            this._validators =
                (validators != null) ? new List<Validator>(validators) : new List<Validator>();

            this._validatables = 
                (validatables != null) ? new List<IValidatable>(validatables) : new List<IValidatable>();
        }

        /// <summary>
        /// Adds the specified validatable.
        /// </summary>
        /// <param name="validatable">The validatable.</param>
        /// <returns>CompositeValidator.</returns>
        public CompositeValidator Add(IValidatable validatable)
        {
            if(validatable != null)
            {
                this._validatables.Add(validatable);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified validatables.
        /// </summary>
        /// <param name="validatables">The validatables.</param>
        /// <returns>CompositeValidator.</returns>
        public CompositeValidator Add(IEnumerable<IValidatable> validatables)
        {
            if(validatables != null)
            {
                this._validatables.AddRange(validatables);
            }

            return this;
        }

        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public override IReadOnlyCollection<ValidationResult> Validate(object? validationSubject)
        {
            List<ValidationResult> results = new();

            if(validationSubject != null)
            {
                foreach(var validator in this._validators)
                {
                    Debug.Assert(validator != null);
                    results.AddRange(validator!.Validate(validationSubject));
                }
            }

            foreach(var validatable in this._validatables)
            {
                Debug.Assert(validatable != null);
                results.AddRange(validatable!.Validate());
            }

            return results;
        }
    }

    /// <summary>
    /// Class Validator (with generic validation subject type).
    /// Implements the <see cref="Rollbar.Common.Validator" />
    /// </summary>
    /// <typeparam name="TValidationSubject">The type of the t validation subject.</typeparam>
    /// <seealso cref="Rollbar.Common.Validator" />
    public abstract class Validator<TValidationSubject>
        : Validator
    {
        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">
        /// The validation subject.
        /// </param>
        /// <returns>
        /// IReadOnlyCollection&lt;ValidationResult&gt;.
        /// </returns>
        public abstract IReadOnlyCollection<ValidationResult> Validate(TValidationSubject? validationSubject);
    }

    /// <summary>
    /// Class Validator (with generic validation subject type and generic validation rule Enum).
    /// Implements the <see cref="Rollbar.Common.Validator{TValidationSubject}" />
    /// </summary>
    /// <typeparam name="TValidationSubject">The type of the t validation subject.</typeparam>
    /// <typeparam name="TValidationRule">The type of the t validation rule.</typeparam>
    /// <seealso cref="Rollbar.Common.Validator{TValidationSubject}" />
    public class Validator<TValidationSubject, TValidationRule>
        : Validator<TValidationSubject>
        where TValidationRule : Enum
    {
        /// <summary>
        /// The validations
        /// </summary>
        private readonly IDictionary<TValidationRule, Func<TValidationSubject, bool>> _validationFunctionsByRule = 
            new Dictionary<TValidationRule, Func<TValidationSubject, bool>>(Validator.DefaultValidationRulesCapacity);

        /// <summary>
        /// The validators by rule
        /// </summary>
        private readonly IDictionary<TValidationRule, Tuple<Validator, LambdaExpression>> _validatorsByRule =
            new Dictionary<TValidationRule, Tuple<Validator, LambdaExpression>>();

        /// <summary>
        /// Adds the validation.
        /// </summary>
        /// <param name="validationRule">The validation rule.</param>
        /// <param name="validationFunc">The validation function.</param>
        /// <returns>Validator&lt;TValidationSubject, TValidationRule&gt;.</returns>
        public Validator<TValidationSubject, TValidationRule> AddValidation(
            TValidationRule validationRule, 
            Func<TValidationSubject, bool> validationFunc
            )
        {
            this._validationFunctionsByRule.Add(validationRule, validationFunc);

            return this;
        }

        /// <summary>
        /// Adds the validation.
        /// </summary>
        /// <typeparam name="TSubjectProperty">The type of the t subject property.</typeparam>
        /// <param name="validationRule">The validation rule.</param>
        /// <param name="subjectPropertyExpression">The subject property expression.</param>
        /// <param name="subjectPropertyValidator">The subject property validator.</param>
        /// <returns>Validator&lt;TValidationSubject, TValidationRule&gt;.</returns>
        public Validator<TValidationSubject, TValidationRule> AddValidation<TSubjectProperty>(
            TValidationRule validationRule,
            Expression<Func<TValidationSubject, TSubjectProperty>> subjectPropertyExpression,
            Validator<TSubjectProperty>? subjectPropertyValidator
            )
        {
            if (subjectPropertyValidator != null)
            {
                this._validatorsByRule[validationRule] =
                    new Tuple<Validator, LambdaExpression>(subjectPropertyValidator, subjectPropertyExpression);
            }

            return this;
        }

        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyDictionary&lt;TValidationRule, ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public override IReadOnlyCollection<ValidationResult> Validate(TValidationSubject? validationSubject)
        {
            CollectorCollection<ValidationResult> failedValidationResults = 
                new(this._validationFunctionsByRule.Count);

            if (validationSubject == null)
            {
                failedValidationResults.Add(new ValidationResult(ValidationRule.ValidationSubjectInstanceRequired));
                return failedValidationResults; // nothing to validate... 
            }

            foreach(var rule in this._validationFunctionsByRule.Keys)
            {
                if (!this._validationFunctionsByRule[rule].Invoke(validationSubject))
                {
                    failedValidationResults.Add(new ValidationResult(rule));
                }
            }

            foreach(var rule in this._validatorsByRule.Keys)
            {
                var validatorAndExpression = this._validatorsByRule[rule];
                Validator validator = validatorAndExpression.Item1;
                LambdaExpression expression = validatorAndExpression.Item2;

                var exprBody = (MemberExpression)expression.Body;
                var property = (PropertyInfo)exprBody.Member;
                object? validationSubjectPropertyValue = property.GetValue(validationSubject);

                var validatorResults = validator.Validate(validationSubjectPropertyValue);

                if (validatorResults.Count > 0)
                {
                    failedValidationResults.Add(new ValidationResult(rule, validatorResults));
                }
            }

            return failedValidationResults;
        }

        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public override IReadOnlyCollection<ValidationResult> Validate(object? validationSubject)
        {
            TValidationSubject? typeSafeValidationSubject;
            try
            {
                typeSafeValidationSubject = (TValidationSubject?) validationSubject;
            }
            catch
            {
                CollectorCollection<ValidationResult> failedValidationResults =
                    new(new ValidationResult[] { new ValidationResult(Validator.ValidationRule.MatchValidationSubjectType), });
                return failedValidationResults;
            }

            return this.Validate(typeSafeValidationSubject);
        }

        /// <summary>
        /// Gets the total validation rules.
        /// </summary>
        /// <value>The total validation rules.</value>
        public int TotalValidationRules
        {
            get
            {
                return (this._validationFunctionsByRule.Keys.Count + this._validatorsByRule.Keys.Count);
            }
        }
    }
}
