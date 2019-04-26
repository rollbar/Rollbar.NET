namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface IValidatable
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;Enum&gt; containing failed validation rules.</returns>
        IReadOnlyCollection<Enum> Validate();
    }

    /// <summary>
    /// Interface IValidatable
    /// Implements the <see cref="Rollbar.Common.IValidatable" />
    /// </summary>
    /// <typeparam name="TValidationRule">The type of the t validation rule.</typeparam>
    /// <seealso cref="Rollbar.Common.IValidatable" />
    public interface IValidatable<out TValidationRule>
        : IValidatable
        where TValidationRule : Enum
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;TValidationRule&gt; containing failed validation rules.</returns>
        new IReadOnlyCollection<TValidationRule> Validate();
    }
}
