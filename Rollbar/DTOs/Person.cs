namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Models Rollbar Person DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    /// <remarks>
    /// Optional: person
    /// The user affected by this event. Will be indexed by ID, username, and email.
    /// People are stored in Rollbar keyed by ID. If you send a multiple different usernames/emails for the
    /// same ID, the last received values will overwrite earlier ones.
    /// </remarks>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Person
        : DtoBase
    {
        private const int maxIdChars = 40;
        private const int maxUsernameChars = 255;
        private const int maxEmailChars = 255;

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public Person()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <remarks>
        /// Required: id
        /// A string up to 40 characters identifying this user in your system.
        /// </remarks>
        public Person(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets or sets the identifier (REQUIRED).
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("id", Required = Required.Always)]
        public string? Id
        {
            get { return this._id; }
            set
            {
                if (value != null && value.Length > Person.maxIdChars)
                {
                    value = value.Substring(0, Person.maxIdChars);
                }
                this._id = value;
            }
        }
        private string? _id;

        /// <summary>
        /// Gets or sets the username (OPTIONAL).
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        /// <remarks>
        /// Optional: username
        /// A string up to 255 characters
        /// </remarks>
        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? UserName
        {
            get { return this._userName; }
            set
            {
                if (value != null && value.Length > Person.maxUsernameChars)
                {
                    value = value.Substring(0, Person.maxUsernameChars);
                }
                this._userName = value;
            }
        }
        private string? _userName;

        /// <summary>
        /// Gets or sets the email (OPTIONAL).
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        /// <remarks>
        /// Optional: email
        /// A string up to 255 characters
        /// </remarks>
        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Email
        {
            get { return this._email; }
            set
            {
                if (value != null && value.Length > Person.maxEmailChars)
                {
                    value = value.Substring(0, Person.maxEmailChars);
                }
                this._email = value;
            }
        }
        private string? _email;

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<Person, Person.PersonValidationRule>()
                    .AddValidation(
                        Person.PersonValidationRule.ValidIdRequired,
                        (person) => { return !string.IsNullOrWhiteSpace(person.Id); }
                        )
                    .AddValidation(
                        Person.PersonValidationRule.IdMaxLengthLimit,
                        (person) => { return !(person.Id?.Length > Person.maxIdChars); }
                        )
                    .AddValidation(
                        Person.PersonValidationRule.UserNameMaxLengthLimit,
                        (person) => { return !(person.UserName?.Length > Person.maxUsernameChars); }
                        )
                    .AddValidation(
                        Person.PersonValidationRule.EmailMaxLengthLimit,
                        (person) => { return !(person.Email?.Length > Person.maxEmailChars); }
                        )
                    ;

            return validator;
        }

        /// <summary>
        /// Enum PersonValidationRule
        /// </summary>
        public enum PersonValidationRule
        {
            /// <summary>
            /// The valid identifier required
            /// </summary>
            ValidIdRequired,

            /// <summary>
            /// The identifier maximum length limit
            /// </summary>
            IdMaxLengthLimit,

            /// <summary>
            /// The user name maximum length limit
            /// </summary>
            UserNameMaxLengthLimit,

            /// <summary>
            /// The email maximum length limit
            /// </summary>
            EmailMaxLengthLimit,
        }

    }
}
