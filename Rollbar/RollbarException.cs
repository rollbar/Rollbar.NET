namespace Rollbar
{
    using System;
    using System.Runtime.Serialization;

    using Rollbar.Infrastructure;

    /// <summary>
    /// Class RollbarException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class RollbarException
        : Exception
        , ISerializable
    {
        /// <summary>
        /// The rollbar error
        /// </summary>
        private readonly InternalRollbarError _rollbarError;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException" /> class.
        /// </summary>
        internal RollbarException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException" /> class.
        /// </summary>
        /// <param name="rollbarError">The rollbar error.</param>
        internal RollbarException(InternalRollbarError rollbarError) 
            : this(rollbarError, rollbarError.ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException"/> class.
        /// </summary>
        /// <param name="rollbarError">The rollbar error.</param>
        /// <param name="innerException">The inner exception.</param>
        internal RollbarException(InternalRollbarError rollbarError, Exception? innerException)
            : this(rollbarError, rollbarError.ToString(), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException"/> class.
        /// </summary>
        /// <param name="rollbarError">The rollbar error.</param>
        /// <param name="message">The message.</param>
        internal RollbarException(InternalRollbarError rollbarError, string? message)
            : base(!string.IsNullOrWhiteSpace(message) ? message : rollbarError.ToString())
        {
            this._rollbarError = rollbarError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException" /> class.
        /// </summary>
        /// <param name="rollbarError">The rollbar error.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        internal RollbarException(InternalRollbarError rollbarError, string? message, Exception? innerException) 
            : base(!string.IsNullOrWhiteSpace(message) ? message : rollbarError.ToString(), innerException)
        {
            this._rollbarError = rollbarError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected RollbarException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this._rollbarError = 
                (InternalRollbarError) (info.GetValue(nameof(this._rollbarError), typeof(InternalRollbarError)) ?? InternalRollbarError.GeneralError);
        }

        /// <summary>
        /// Gets the rollbar error.
        /// </summary>
        /// <value>The rollbar error.</value>
        public InternalRollbarError RollbarError
        {
            get { return this._rollbarError; }
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo">SerializationInfo</see> with information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo">SerializationInfo</see> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext">StreamingContext</see> that contains contextual information about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(this._rollbarError), this._rollbarError);
        }

    }
}
