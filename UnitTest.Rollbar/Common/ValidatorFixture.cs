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

    [TestClass]
    [TestCategory(nameof(ValidatorFixture))]
    public class ValidatorFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        public class PersonMock
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }

            public enum PersonValidationRule
            {
                ValidIdRequired,
                IdMaxLengthLimit,
                UserNameMaxLengthLimit,
                EmailMaxLengthLimit,
            }
        }

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
        public void TestWithSpecificValidationRules(string personId, string userName, string email, int expectedTotalFailedRules)
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

        public class ConfigMock
        {
            public string Id { get; set; }
            public string Environment { get; set; }

            public PersonMock User { get; set; }

            public enum ConfigValidationRule
            {
                ValidIdRequired,
                IdMaxLengthLimit,
                EnvironmentMaxLengthLimit,
                UserRequired,
                ValidUser,
            }
        }

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
        public void TestWithAbstractValidationRules(string token, string env, string personId, string userName, string email, int expectedTotalFailedRules)
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
