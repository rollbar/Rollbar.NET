namespace Rollbar.Infrastructure
{
    using System;

    using Rollbar.DTOs;
    using Rollbar.PayloadTruncation;

    /// <summary>
    /// Class RollbarPayloadTruncator.
    /// </summary>
    internal class RollbarPayloadTruncator
    {
        /// <summary>
        /// The rollbar
        /// </summary>
        private readonly IRollbar? _rollbar;

        /// <summary>
        /// The truncation strategy
        /// </summary>
        private readonly IterativeTruncationStrategy _truncationStrategy = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarPayloadTruncator"/> class from being created.
        /// </summary>
        private RollbarPayloadTruncator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPayloadTruncator"/> class.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        public RollbarPayloadTruncator(IRollbar? rollbar)
        {
            this._rollbar = rollbar;
        }

        /// <summary>
        /// Truncates the payload.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if truncated successfully, <c>false</c> otherwise.</returns>
        public bool TruncatePayload(PayloadBundle payloadBundle)
        {
            Payload? payload = payloadBundle.GetPayload();
            if (payload == null) 
                return false;

            if (this._truncationStrategy.Truncate(payload) > this._truncationStrategy.MaxPayloadSizeInBytes)
            {
                var exception = new ArgumentOutOfRangeException(
                    paramName: nameof(payloadBundle),
                    message: $"Bundle's payload size exceeds {this._truncationStrategy.MaxPayloadSizeInBytes} bytes limit!"
                );

                RollbarErrorUtility.Report(
                    this._rollbar,
                    payload,
                    InternalRollbarError.PayloadTruncationError,
                    "While truncating a payload...",
                    exception,
                    payloadBundle
                );

                return false;
            }

            return true;
        }

    }
}
