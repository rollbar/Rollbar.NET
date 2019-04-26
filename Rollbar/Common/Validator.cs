namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Class Validator.
    /// </summary>
    public abstract class Validator
    {
        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public abstract IReadOnlyCollection<ValidationResult> Validate(object validationSubject);

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
            new Dictionary<TValidationRule, Func<TValidationSubject, bool>>(Validator<TValidationSubject, TValidationRule>.DefaultValidationRulesCapacity);

        /// <summary>
        /// The validators by rule
        /// </summary>
        private readonly IDictionary<TValidationRule, Tuple<Validator, LambdaExpression>> _validatorsByRule =
            new Dictionary<TValidationRule, Tuple<Validator, LambdaExpression>>();

        /// <summary>
        /// Gets or sets the default validation rules capacity.
        /// </summary>
        /// <value>The default validation rules capacity.</value>
        public static int DefaultValidationRulesCapacity { get; set; } = 10;

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
            Validator<TSubjectProperty> subjectPropertyValidator
            )
        {
            this._validatorsByRule[validationRule] = 
                new Tuple<Validator, LambdaExpression>(subjectPropertyValidator, subjectPropertyExpression);

            return this;
        }

        /// <summary>
        /// Validates the specified validation subject.
        /// </summary>
        /// <param name="validationSubject">The validation subject.</param>
        /// <returns>IReadOnlyDictionary&lt;TValidationRule, ValidationResult&gt; containing failed validation rules with optional details as values.</returns>
        public IReadOnlyCollection<ValidationResult> Validate(TValidationSubject validationSubject)
        {
            CollectorCollection<ValidationResult> failedValidationResults = 
                new CollectorCollection<ValidationResult>(this._validationFunctionsByRule.Count);

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
                object validationSubjectPropertyValue = property.GetValue(validationSubject);

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
        public override IReadOnlyCollection<ValidationResult> Validate(object validationSubject)
        {
            TValidationSubject typeSafeValidationSubject = default(TValidationSubject);
            try
            {
                typeSafeValidationSubject = (TValidationSubject)validationSubject;
            }
            catch
            {
                CollectorCollection<ValidationResult> failedValidationResults =
                    new CollectorCollection<ValidationResult>(new ValidationResult[] { new ValidationResult(Validator.ValidationRule.MatchValidationSubjectType), });
                return failedValidationResults;
            }

            return this.Validate(typeSafeValidationSubject);
        }
    }
}
