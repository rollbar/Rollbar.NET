namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class MessagePackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class MessagePackage
        : RollbarPackageBase
    {
        /// <summary>
        /// The message to package
        /// </summary>
        private readonly string? _messageToPackage;

        /// <summary>
        /// The rollbar data title
        /// </summary>
        private readonly string? _rollbarDataTitle;

        /// <summary>
        /// The extra information
        /// </summary>
        private readonly IDictionary<string, object?>? _extraInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackage"/> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        public MessagePackage(
            string messageToPackage
            )
            : this(messageToPackage, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackage"/> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        public MessagePackage(
            string messageToPackage, 
            string rollbarDataTitle
            )
            : this(messageToPackage, rollbarDataTitle, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackage" /> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="extraInfo">The extra information.</param>
        public MessagePackage(
            string? messageToPackage, 
            IDictionary<string, object?>? extraInfo
            )
            : this(messageToPackage, null, extraInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackage" /> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="extraInfo">The extra information.</param>
        public MessagePackage(
            string? messageToPackage, 
            string? rollbarDataTitle, 
            IDictionary<string, object?>? extraInfo
            )
            : base(false)
        {
            Assumption.AssertNotNull(messageToPackage, nameof(messageToPackage));

            this._messageToPackage = messageToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
            this._extraInfo = extraInfo;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            Message rollbarMessage = new Message(this._messageToPackage, this._extraInfo);
            Body rollbarBody = new Body(rollbarMessage);
            Data rollbarData = new Data(rollbarBody);
            rollbarData.Title = this._rollbarDataTitle;

            return rollbarData;
        }
    }
}
