namespace Rollbar
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Class RollbarDataScrubbingHelper.
    /// </summary>
    /// <remarks>
    /// This type is intended to be extended (via the C# type extension methods)
    /// by the optional Rollbar extension components with other relevant getter methods
    /// defining other relevant common data fields to scrub.
    /// </remarks>
    public class RollbarDataScrubbingHelper
    {

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarDataScrubbingHelper Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarQueueController" /> class from being created.
        /// </summary>
        private RollbarDataScrubbingHelper()
        {
        }

        /// <summary>
        /// Class NestedSingleInstance. This class cannot be inherited.
        /// </summary>
        private sealed class NestedSingleInstance
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NestedSingleInstance" /> class from being created.
            /// </summary>
            private NestedSingleInstance()
            {
            }

            /// <summary>
            /// The instance
            /// </summary>
            internal static readonly RollbarDataScrubbingHelper Instance =
                new RollbarDataScrubbingHelper();
        }

        #endregion singleton implementation

        /// <summary>
        /// Combines the specified data field sets.
        /// </summary>
        /// <param name="dataFieldSets">The data field sets.</param>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public static ISet<string> Combine(IEnumerable<ISet<string>> dataFieldSets)
        {
            //int capacity = dataFieldSets.Aggregate(0, (total, next) => total += next.Count);
            //HashSet<string> resultingSet = new HashSet<string>(capacity);
            HashSet<string> resultingSet = new HashSet<string>();

            foreach (var dataFieldSet in dataFieldSets)
            {
                resultingSet.UnionWith(dataFieldSet);
            }

            return resultingSet;
        }

        /// <summary>
        /// Combines the specified data field sets.
        /// </summary>
        /// <param name="dataFieldSets">The data field sets.</param>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public static ISet<string> Combine(params ISet<string>[] dataFieldSets)
        {
            return RollbarDataScrubbingHelper.Combine(dataFieldSets.AsEnumerable());
        }

        /// <summary>
        /// Removes the specified fields from the initial data field set.
        /// </summary>
        /// <param name="initialDataFieldSet">The initial data field set.</param>
        /// <param name="dataFieldsToRemove">The data fields to remove.</param>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public static ISet<string> Remove(ISet<string> initialDataFieldSet, IEnumerable<string> dataFieldsToRemove)
        {
            HashSet<string> resultingSet = new HashSet<string>(initialDataFieldSet);
            resultingSet.ExceptWith(dataFieldsToRemove);
            return resultingSet;
        }

        /// <summary>
        /// Gets the common password fields.
        /// </summary>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public virtual ISet<string> GetCommonPasswordFields()
        {
            return new HashSet<string>(
                new[]
                {
                    "passwd",
                    "Passwd",
                    "password",
                    "Password",
                    "secret",
                    "Secret",
                    "confirm_password",
                    "confirmPassword",
                    "ConfirmPassword",
                    "password_confirmation",
                    "passwordConfirmation",
                    "PasswordConfirmation",
                }
                );
        }

        /// <summary>
        /// Gets the common credit card number fields.
        /// </summary>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public virtual ISet<string> GetCommonCreditCardNumberFields()
        {
            return new HashSet<string>(
                new[]
                {
                    "card_number",
                    "CardNumber",
                    "cardNumber",
                }
            );
        }

        /// <summary>
        /// Gets the common credit card CVV fields.
        /// </summary>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public virtual ISet<string> GetCommonCreditCardCvvFields()
        {
            return new HashSet<string>(
                new[]
                {
                    "cvv",
                    "CVV",
                    "Cvv",

                    "card_cvv",
                    "cardCVV",
                    "CardCVV",
                    "cardCvv",
                    "CardCvv",
                }
            );
        }

        /// <summary>
        /// Gets the common HTTP header fields.
        /// </summary>
        /// <returns>ISet&lt;System.String&gt;.</returns>
        public virtual ISet<string> GetCommonHttpHeaderFields()
        {
            return new HashSet<string>(
                new[]
                {
                    "authorization",
                    "www-authorization",
                    "http_authorization",
                    "omniauth.auth",
                    "cookie",
                    "oauth-access-token",
                    "x-access-token",
                    "x_csrf_token",
                    "http_x_csrf_token",
                    "x-csrf-token",
                }
            );
        }

    }
}
