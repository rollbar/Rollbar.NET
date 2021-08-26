namespace Rollbar
{
    using System;
    using Rollbar.DTOs;

    /// <summary>
    /// Class PersonPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class PersonPackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The person
        /// </summary>
        private readonly Person _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPackageDecorator" /> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personUsername">The person username.</param>
        /// <param name="personEmail">The person email.</param>
        public PersonPackageDecorator(
            IRollbarPackage packageToDecorate,
            string personId,
            string personUsername,
            string? personEmail = null
            )
            : this(packageToDecorate, new Person(personId) { UserName = personUsername, Email = personEmail })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPackageDecorator" /> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="person">The person.</param>
        public PersonPackageDecorator(
            IRollbarPackage packageToDecorate,
            Person person
            ) 
            : base(packageToDecorate, false)
        {
            this._person = person;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data? rollbarData)
        {
            if(rollbarData == null)
            {
                return;
            }

            rollbarData.Person = this._person;
        }
    }
}
