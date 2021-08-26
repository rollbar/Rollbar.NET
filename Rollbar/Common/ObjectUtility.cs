namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Class ObjectUtility.
    /// </summary>
    public static class ObjectUtility
    {
        /// <summary>
        /// Ares the similar references.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if references have similar values, <c>false</c> otherwise.</returns>
        public static bool AreSimilarReferences(object left, object right)
        {
            if(left == right)
            {
                return true;
            }
            else if(left == null && right == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Ares the comparable via properties.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if objects are comparable based on their property values, <c>false</c> otherwise.</returns>
        public static bool AreComparableViaProperties(object left,object right)
        {
            if (!(left != null && right !=null))
            {
                return false;
            }

            var leftType = left.GetType();
            var rightType = right.GetType();

            if (ReflectionUtility.GetTopCommonSuperType(leftType, rightType) != typeof(object))
            {
                return true;
            }

            if (ReflectionUtility.GetCommonImplementedInterfaces(leftType, rightType).Length > 0)
            {
                return true;
            }

            return false;
        }
    }
}
