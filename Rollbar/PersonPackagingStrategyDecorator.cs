﻿namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    /// <summary>
    /// Class PersonPackagingStrategyDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyDecoratorBase" />
    public class PersonPackagingStrategyDecorator
        : RollbarPackagingStrategyDecoratorBase
    {
        /// <summary>
        /// The person
        /// </summary>
        private readonly Person _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPackagingStrategyDecorator"/> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personUsername">The person username.</param>
        /// <param name="personEmail">The person email.</param>
        public PersonPackagingStrategyDecorator(
            IRollbarPackagingStrategy strategyToDecorate,
            string personId,
            string personUsername,
            string personEmail
            )
            : this(strategyToDecorate, new Person(personId) { UserName = personUsername, Email = personEmail })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPackagingStrategyDecorator"/> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="person">The person.</param>
        public PersonPackagingStrategyDecorator(
            IRollbarPackagingStrategy strategyToDecorate,
            Person person
            ) 
            : base(strategyToDecorate, false)
        {
            this._person = person;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public override Data PackageAsRollbarData()
        {
            Data rollbarData = base.PackageAsRollbarData();
            rollbarData.Person = this._person;
            return rollbarData;
        }
    }
}