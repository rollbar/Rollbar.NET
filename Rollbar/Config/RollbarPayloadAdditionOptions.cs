namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarPayloadAdditionOptions.
    /// Implements the <see cref="ReconfigurableBase{T, TBase}" />
    /// Implements the <see cref="IRollbarPayloadAdditionOptions" />
    /// </summary>
    /// <seealso cref="ReconfigurableBase{T, TBase}" />
    /// <seealso cref="IRollbarPayloadAdditionOptions" />
    public class RollbarPayloadAdditionOptions
        : ReconfigurableBase<RollbarPayloadAdditionOptions, IRollbarPayloadAdditionOptions>
        , IRollbarPayloadAdditionOptions
    {
        private const Person defaultPerson = null;
        private const Server defaultServer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPayloadAdditionOptions"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="server">The server.</param>
        public RollbarPayloadAdditionOptions(
            Person person = RollbarPayloadAdditionOptions.defaultPerson, 
            Server server = RollbarPayloadAdditionOptions.defaultServer
            )
        {
            Person = person;
            Server = server;
        }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        public Person? Person
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>The server.</value>
        public Server? Server
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the code version.
        /// </summary>
        /// <value>
        /// The code version.
        /// </value>
        public string? CodeVersion
        {
            get; 
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarPayloadAdditionOptions Reconfigure(
            IRollbarPayloadAdditionOptions likeMe
            )
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            Validator<Person?>? personValidator = this.Person?.GetValidator() as Validator<Person?>;

            if(personValidator != null)
            {
                var validator =
                    new Validator<RollbarPayloadAdditionOptions, RollbarPayloadAdditionOptions.RollbarPayloadAdditionOptionsValidationRule>()
                        .AddValidation(
                            RollbarPayloadAdditionOptions.RollbarPayloadAdditionOptionsValidationRule.ValidPersonIfAny,
                            (config) => config.Person,
                            personValidator
                            )
                   ;

                return validator;
            }

            return null;
        }

        /// <summary>
        /// Enum RollbarPayloadAdditionOptionsValidationRule
        /// </summary>
        public enum RollbarPayloadAdditionOptionsValidationRule
        {
            /// <summary>
            /// The valid person (if any)
            /// </summary>
            ValidPersonIfAny,
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarPayloadAdditionOptions IReconfigurable<IRollbarPayloadAdditionOptions, IRollbarPayloadAdditionOptions>.Reconfigure(
            IRollbarPayloadAdditionOptions likeMe
            )
        {
            return this.Reconfigure(likeMe);
        }
    }
}
