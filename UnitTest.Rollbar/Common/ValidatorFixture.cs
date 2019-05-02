namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines test class ValidatorFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(ValidatorFixture))]
    public class ValidatorFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
        }

        /// <summary>
        /// Class PersonMock.
        /// </summary>
        public class PersonMock
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public string Id { get; set; }
            /// <summary>
            /// Gets or sets the name of the user.
            /// </summary>
            /// <value>The name of the user.</value>
            public string UserName { get; set; }
            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>The email.</value>
            public string Email { get; set; }

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

        /// <summary>
        /// Tests the with specific validation rules.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="expectedTotalFailedRules">The expected total failed rules.</param>
        [DataTestMethod]
        [DataRow("ID1", null, null, 0)]
        [DataRow("TooLongID1", null, null, 1)]
        [DataRow(null, null, null, 1)]
        [DataRow("ID1", "SomeName", null, 0)]
        [DataRow("ID1", "SomeNameTooLong", null, 1)]
        [DataRow("ID1", "SomeName", "e@mail.com", 0)]
        [DataRow("ID1", "SomeName", "TooLong@mail.com", 1)]
        [DataRow("ID1", "SomeNameTooLong", "TooLong@mail.com", 2)]
        [DataRow("TooLongID1", "SomeNameTooLong", "TooLong@mail.com", 3)]
        [DataRow(null, "SomeNameTooLong", "TooLong@mail.com", 3)]
        public void TestSimpleObjectValidation(string personId, string userName, string email, int expectedTotalFailedRules)
        {
            Validator<PersonMock, PersonMock.PersonValidationRule> personValidator =
                new Validator<PersonMock, PersonMock.PersonValidationRule>()
                .AddValidation(
                    PersonMock.PersonValidationRule.ValidIdRequired, 
                    (person) => { return !string.IsNullOrWhiteSpace(person.Id); }
                    )
                .AddValidation(
                    PersonMock.PersonValidationRule.IdMaxLengthLimit, 
                    (person) => { return !(person.Id?.Length > 5); }
                    )
                .AddValidation(
                    PersonMock.PersonValidationRule.UserNameMaxLengthLimit, 
                    (person) => { return !(person.UserName?.Length > 10); }
                    )
                .AddValidation(
                    PersonMock.PersonValidationRule.EmailMaxLengthLimit, 
                    (person) => { return !(person.Email?.Length > 10); }
                    )
                ;

            PersonMock validationSubject = new PersonMock() { Id = personId, UserName = userName, Email = email, };
            var failedValidationResults = personValidator.Validate(validationSubject);
            Assert.AreEqual(expectedTotalFailedRules, failedValidationResults.Count);
        }

        /// <summary>
        /// Class ConfigMock.
        /// </summary>
        public class ConfigMock
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public string Id { get; set; }
            /// <summary>
            /// Gets or sets the environment.
            /// </summary>
            /// <value>The environment.</value>
            public string Environment { get; set; }

            /// <summary>
            /// Gets or sets the user.
            /// </summary>
            /// <value>The user.</value>
            public PersonMock User { get; set; }

            /// <summary>
            /// Enum ConfigValidationRule
            /// </summary>
            public enum ConfigValidationRule
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
                /// The environment maximum length limit
                /// </summary>
                EnvironmentMaxLengthLimit,
                /// <summary>
                /// The user required
                /// </summary>
                UserRequired,
                /// <summary>
                /// The valid user
                /// </summary>
                ValidUser,
            }
        }

        /// <summary>
        /// Tests the with abstract validation rules.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="env">The env.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="expectedTotalFailedRules">The expected total failed rules.</param>
        [DataTestMethod]
        [DataRow("Token1", "Env1", "ID1", null, null, 0)]
        [DataRow(null, "Env1", "TooLongID1", null, null, 2)]
        [DataRow("Token1", "LongEnv1", null, null, null, 2)]
        [DataRow("LongToken1", "Env1", "ID1", "SomeName", null, 1)]
        [DataRow("Token1", "Env1", "ID1", "SomeNameTooLong", null, 1)]
        [DataRow("Token1", "Env1", "ID1", "SomeName", "e@mail.com", 0)]
        [DataRow("LongToken1", "LongEnv1", "ID1", "SomeName", "TooLong@mail.com", 3)]
        [DataRow("Token1", "Env1", "ID1", "SomeNameTooLong", "TooLong@mail.com", 1)]
        [DataRow("Token1", "LongEnv1", "TooLongID1", "SomeNameTooLong", "TooLong@mail.com", 2)]
        [DataRow("LongToken1", "LongEnv1", null, "SomeNameTooLong", "TooLong@mail.com", 3)]
        public void TestStructuredObjectValidation(string token, string env, string personId, string userName, string email, int expectedTotalFailedRules)
        {
            Validator<PersonMock, PersonMock.PersonValidationRule> personValidator =
                new Validator<PersonMock, PersonMock.PersonValidationRule>()
                    .AddValidation(
                        PersonMock.PersonValidationRule.ValidIdRequired,
                        (person) => { return !string.IsNullOrWhiteSpace(person.Id); }
                        )
                    .AddValidation(
                        PersonMock.PersonValidationRule.IdMaxLengthLimit,
                        (person) => { return !(person.Id?.Length > 5); }
                        )
                    .AddValidation(
                        PersonMock.PersonValidationRule.UserNameMaxLengthLimit,
                        (person) => { return !(person.UserName?.Length > 10); }
                        )
                    .AddValidation(
                        PersonMock.PersonValidationRule.EmailMaxLengthLimit,
                        (person) => { return !(person.Email?.Length > 10); }
                        )
                    ;

            Validator<ConfigMock, ConfigMock.ConfigValidationRule> configValidator =
                new Validator<ConfigMock, ConfigMock.ConfigValidationRule>()
                        .AddValidation(
                            ConfigMock.ConfigValidationRule.ValidIdRequired,
                            (config) => { return !string.IsNullOrWhiteSpace(config.Id); }
                            )
                        .AddValidation(
                            ConfigMock.ConfigValidationRule.IdMaxLengthLimit,
                            (config) => { return !(config.Id?.Length > 6); }
                            )
                        .AddValidation(
                            ConfigMock.ConfigValidationRule.EnvironmentMaxLengthLimit,
                            (config) => { return !(config.Environment?.Length > 5); }
                            )
                        .AddValidation(
                            ConfigMock.ConfigValidationRule.UserRequired,
                            (config) => { return config.User != null; }
                            )
                        .AddValidation(ConfigMock.ConfigValidationRule.ValidUser,
                            (config) => config.User,
                            personValidator
                            )
                   ;

            PersonMock userMock = new PersonMock() { Id = personId, UserName = userName, Email = email, };
            ConfigMock configMock = new ConfigMock() { Id = token, Environment = env, User = userMock, };
            var failedValidationRules = configValidator.Validate(configMock);
            Assert.AreEqual(expectedTotalFailedRules, failedValidationRules.Count);
        }
    }
}
