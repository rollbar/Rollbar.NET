namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Enum InternalRollbarError
    /// </summary>
    public enum InternalRollbarError
    {
        /// <summary>
        /// The general error
        /// </summary>
        GeneralError,

        /// <summary>
        /// The configuration error
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// The packaging error
        /// </summary>
        PackagingError,

        /// <summary>
        /// The bundling error
        /// </summary>
        BundlingError,

        /// <summary>
        /// The enqueuing error
        /// </summary>
        EnqueuingError,

        /// <summary>
        /// The dequeuing error
        /// </summary>
        DequeuingError,

        /// <summary>
        /// The payload check ignore error
        /// </summary>
        PayloadCheckIgnoreError,

        /// <summary>
        /// The payload transform error
        /// </summary>
        PayloadTransformError,

        /// <summary>
        /// The payload truncation error
        /// </summary>
        PayloadTruncationError,

        /// <summary>
        /// The payload validation error
        /// </summary>
        PayloadValidationError,

        /// <summary>
        /// The payload serialization error
        /// </summary>
        PayloadSerializationError,

        /// <summary>
        /// The payload scrubbing error
        /// </summary>
        PayloadScrubbingError,
    }
}
