namespace Rollbar.Diagnostics
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Utility class aiding in validating assumptions about arguments and their values.
    /// </summary>
    public static class Assumption
    {

        /// <summary>
        /// Fails the validation.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void FailValidation(string msg, string argumentName)
        {
            //Debug.Assert(false, msg);
            throw new ArgumentException(msg, argumentName);
        }


        /// <summary>
        /// Asserts the  generic type parameter as an interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="System.ArgumentException">Generic type parameter should be an interface only.</exception>
        public static void AssertIsInterface<T>() where T : class
        {
            if (!typeof(T).IsInterface)
            {
                string msg = "Generic type parameter should be an interface type.";
                FailValidation(msg, typeof(T).Name);
            }
        }

        /// <summary>
        /// Asserts the true.
        /// </summary>
        /// <param name="testedValue">if set to <c>true</c> [tested value].</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Argument should be equal to  + true</exception>
        public static bool AssertTrue(bool testedValue, string parameterName)
        {
            if (!testedValue)
            {
                string msg = "Argument should be equal to " + "true";
                FailValidation(msg, parameterName);
            }

            return testedValue;
        }

        /// <summary>
        /// Asserts the false.
        /// </summary>
        /// <param name="testedValue">if set to <c>true</c> [tested value].</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Argument should be equal to  + false</exception>
        public static bool AssertFalse(bool testedValue, string parameterName)
        {
            if (testedValue)
            {
                string msg = "Argument should be equal to " + "false";
                FailValidation(msg, parameterName);
            }

            return testedValue;
        }

        /// <summary>
        /// Asserts the not equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="testedValue">The tested value.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Argument should not be equal to  + compareToValue</exception>
        public static T AssertNotEqual<T>(T testedValue, T compareToValue, string parameterName)
            where T : IEquatable<T>
        {
            if (testedValue.Equals(compareToValue))
            {
                string msg = "Argument should not be equal to " + compareToValue;
                FailValidation(msg, parameterName);
            }

            return testedValue;
        }

        /// <summary>
        /// Asserts the equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="testedValue">The tested value.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Argument should be equal to  + compareToValue</exception>
        public static T AssertEqual<T>(T testedValue, T compareToValue, string parameterName)
            where T : IEquatable<T>
        {
            if (!testedValue.Equals(compareToValue))
            {
                string msg = "Argument should be equal to " + compareToValue;
                FailValidation(msg, parameterName);
            }

            return testedValue;
        }

        /// <summary>
        /// Utility class for validating method parameters.
        /// </summary>
        /// 		/// <summary>
        /// Ensures the specified value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The specified value.</returns>
        /// <exception cref="ArgumentNullException">Occurs if the specified value 
        /// is <code>null</code>.</exception>
        /// <example>
        /// public UIElementAdapter(UIElement uiElement)
        /// {
        /// 	this.uiElement = Assumption.AssertNotNull(uiElement, "uiElement");	
        /// }
        /// </example>
        public static T AssertNotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                string msg = "Argument should not be NULL";
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Ensures the specified value is not <code>null</code> or empty (a zero length string).
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The specified value.</returns>
        /// <exception cref="ArgumentNullException">Occurs if the specified value 
        /// is <code>null</code> or empty (a zero length string).</exception>
        /// <example>
        /// public DoSomething(string message)
        /// {
        /// 	this.message = Assumption.AssertNotNullOrEmpty(message, "message");	
        /// }
        /// </example>
        public static string AssertNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
            {
                string msg = "Argument should not be NULL.";
                FailValidation(msg, parameterName);
            }

            if (string.IsNullOrEmpty(value))
            {
                string msg = "Argument should not be an empty string.";
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public static string AssertEqual(string value, string expectedValue, bool ignoreCase, string parameterName)
        {
            if (value == null && expectedValue == null)
                return null;
            if (value == null || expectedValue == null)
            {
                string msg = $"Argument is expected to be equal to {expectedValue}, while it is actually: {value}.";
                FailValidation(msg, parameterName);
            }
            if (string.Compare(value, expectedValue, ignoreCase) != 0)
            {
                string msg = $"Argument is expected to be equal to {expectedValue}, while it is actually: {value}.";
                FailValidation(msg, parameterName);
            }
            return value;
        }
        /// <summary>
        /// Ensures the specified value is not <code>null</code> 
        /// or white space.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>The specified value.</returns>
        /// <exception cref="ArgumentNullException">Occurs if the specified value 
        /// is <code>null</code> or consists of only white space.</exception>
        public static string AssertNotNullOrWhiteSpace(string value, string parameterName)
        {
            if (value == null || value.Trim().Length == 0)
            {
                string msg = "Parameter should not be null or white space.";
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts that the specified value is not an empty guid.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="parameterName">The name of the member.</param>
        /// <returns>The specified value.</returns>
        /// <exception cref="ArgumentNullException">Occurs if the specified value 
        /// is an empty guid. That is, if <c>value</c> equals <c>Guid.Empty</c>.</exception>
        public static Guid AssertNotEmpty(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
            {
                string msg = "Parameter should not be an empty Guid.";
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts that the specified value is greater
        /// than the specified expected value.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">The name of the member.</param>
        /// <returns>
        /// The specified value.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than  + expectedValue</exception>
        /// <exception cref="ArgumentNullException">Occurs if the specified value
        /// is not greater than the expected value.</exception>
        public static int AssertGreaterThan(int value, int expectedValue, string parameterName)
        {
            if (value <= expectedValue)
            {
                string msg = "Argument should be greater than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the greater than.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than  + expectedValue</exception>
        public static double AssertGreaterThan(double value, double expectedValue, string parameterName)
        {
            if (value <= expectedValue)
            {
                string msg = "Argument should be greater than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the greater than.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than  + expectedValue</exception>
        public static DateTimeOffset AssertGreaterThan(DateTimeOffset value, DateTimeOffset expectedValue, string parameterName)
        {
            if (value <= expectedValue)
            {
                string msg = "Argument should be greater than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the greater than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than or equal to  + expectedValue</exception>
        public static int AssertGreaterThanOrEqual(int value, int expectedValue, string parameterName)
        {
            if (value < expectedValue)
            {
                string msg = "Argument should be greater than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the greater than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than or equal to  + expectedValue</exception>
        public static double AssertGreaterThanOrEqual(double value, double expectedValue, string parameterName)
        {
            if (value < expectedValue)
            {
                string msg = "Argument should be greater than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the greater than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be greater than or equal to  + expectedValue</exception>
        public static DateTimeOffset AssertGreaterThanOrEqual(DateTimeOffset value, DateTimeOffset expectedValue, string parameterName)
        {
            if (value < expectedValue)
            {
                string msg = "Argument should be greater than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than  + expectedValue</exception>
        public static int AssertLessThan(int value, int expectedValue, string parameterName)
        {
            if (value < expectedValue)
            {
                string msg = "Argument should be less than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than  + expectedValue</exception>
        public static double AssertLessThan(double value, double expectedValue, string parameterName)
        {
            if (value < expectedValue)
            {
                string msg = "Argument should be less than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than  + expectedValue</exception>
        public static DateTimeOffset AssertLessThan(DateTimeOffset value, DateTimeOffset expectedValue, string parameterName)
        {
            if (value >= expectedValue)
            {
                string msg = "Argument should be less than " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than or equal to  + expectedValue</exception>
        public static int AssertLessThanOrEqual(int value, int expectedValue, string parameterName)
        {
            if (value <= expectedValue)
            {
                string msg = "Argument should be less than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than or equal to  + expected</exception>
        public static double AssertLessThanOrEqual(double value, double expectedValue, string parameterName)
        {
            if (value <= expectedValue)
            {
                string msg = "Argument should be less than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Asserts the less than or equal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Argument should be less than or equal to  + expected</exception>
        public static DateTimeOffset AssertLessThanOrEqual(DateTimeOffset value, DateTimeOffset expectedValue, string parameterName)
        {
            if (value > expectedValue)
            {
                string msg = "Argument should be less than or equal to " + expectedValue;
                FailValidation(msg, parameterName);
            }

            return value;
        }

        /// <summary>
        /// Ensures the specified value is not <code>null</code> 
        /// and that it is of the specified type.
        /// </summary>
        /// <param name="value">The value to test.</param> 
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The value to test.</returns>
        /// <exception cref="ArgumentNullException">Occurs if the specified value 
        /// is <code>null</code> or of type not assignable from the specified type.</exception>
        /// <example>
        /// public DoSomething(object message)
        /// {
        /// 	this.message = Assumption.AssertNotNullAndOfType&lt;string&gt;(message, "message");	
        /// }
        /// </example>
        public static T AssertNotNullAndOfType<T>(object value, string parameterName) where T : class
        {
            if (value == null)
            {
                string msg = string.Format("Expected argument '{0}' can not be NULL.", parameterName);
                FailValidation(msg, parameterName);
            }

            var result = value as T;
            if (result == null)
            {
                string msg = string.Format("Expected argument of type " + typeof(T) + ", but was " + value.GetType(), typeof(T), value.GetType());
                FailValidation(msg, parameterName);
            }

            return result;
        }

    }
}
