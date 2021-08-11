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
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules.</returns>
        IReadOnlyCollection<ValidationResult> Validate();

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        Validator? GetValidator();
    }
}
