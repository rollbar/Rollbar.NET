namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class MessagePackagingStrategy.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyBase" />
    public class MessagePackagingStrategy
        : RollbarPackagingStrategyBase
    {
        /// <summary>
        /// The message to package
        /// </summary>
        private readonly string _messageToPackage;

        /// <summary>
        /// The rollbar data title
        /// </summary>
        private readonly string _rollbarDataTitle;

        /// <summary>
        /// The extra information
        /// </summary>
        private readonly IDictionary<string, object> _extraInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackagingStrategy"/> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        public MessagePackagingStrategy(
            string messageToPackage, 
            string rollbarDataTitle
            )
            : this(messageToPackage, rollbarDataTitle, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackagingStrategy" /> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="extraInfo">The extra information.</param>
        public MessagePackagingStrategy(
            string messageToPackage, 
            IDictionary<string, object> extraInfo
            )
            : this(messageToPackage, null, extraInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackagingStrategy" /> class.
        /// </summary>
        /// <param name="messageToPackage">The message to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="extraInfo">The extra information.</param>
        public MessagePackagingStrategy(
            string messageToPackage, 
            string rollbarDataTitle, 
            IDictionary<string, object> extraInfo
            )
            : base(false)
        {
            Assumption.AssertNotNull(messageToPackage, nameof(messageToPackage));

            this._messageToPackage = messageToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
            this._extraInfo = extraInfo;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public override Data PackageAsRollbarData()
        {
            Message rollbarMessage = new Message(this._messageToPackage, this._extraInfo);
            Body rollbarBody = new Body(rollbarMessage);
            Data rollbarData = new Data(rollbarBody);
            rollbarData.Title = this._rollbarDataTitle;

            return rollbarData;
        }
    }
}
