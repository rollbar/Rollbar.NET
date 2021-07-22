namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;
    using Rollbar.DTOs;

    public class RollbarPayloadAdditionOptions
        : ReconfigurableBase<RollbarPayloadAdditionOptions, IRollbarPayloadAdditionOptions>
        , IRollbarPayloadAdditionOptions
    {
        private const Person defaultPerson = null;
        private const Server defaultServer = null;

        public RollbarPayloadAdditionOptions(
            Person person = RollbarPayloadAdditionOptions.defaultPerson, 
            Server server = RollbarPayloadAdditionOptions.defaultServer
            )
        {
            Person = person;
            Server = server;
        }

        public Person Person
        {
            get;
            set;
        }
        public Server Server
        {
            get;
            set;
        }

        public IRollbarPayloadAdditionOptions Reconfigure(IRollbarPayloadAdditionOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            var validator = 
                new Validator<RollbarPayloadAdditionOptions, RollbarPayloadAdditionOptions.RollbarPayloadAdditionOptionsValidationRule>()
                    .AddValidation(
                        RollbarPayloadAdditionOptions.RollbarPayloadAdditionOptionsValidationRule.ValidPersonIfAny,
                        (config) => config.Person,
                        this.Person?.GetValidator() as Validator<Person>
                        )
               ;

            return validator;
        }

        public enum RollbarPayloadAdditionOptionsValidationRule
        {
            /// <summary>
            /// The valid person (if any)
            /// </summary>
            ValidPersonIfAny,
        }

    }
}
