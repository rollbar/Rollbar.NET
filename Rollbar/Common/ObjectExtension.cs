namespace Rollbar.Common
{
    using System;
    using System.Text;
    using System.Reflection;

    /// <summary>
    /// Class ObjectExtension.
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// The default indentation
        /// </summary>
        private const string defaultIndentation = "  ";

        /// <summary>
        /// Renders as string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        public static string RenderAsString(this object obj)
        {
            return RenderAsString(obj, null);
        }

        /// <summary>
        /// Renders as string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="indentation">The indentation.</param>
        /// <returns>System.String.</returns>
        public static string RenderAsString(this object obj, string indentation)
        {
            return RenderAsString(obj, indentation, new StringBuilder());
        }

        /// <summary>
        /// Renders as a string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="indentation">The indentation.</param>
        /// <param name="stringBuilder">The string builder.</param>
        /// <returns>System.String.</returns>
        private static string RenderAsString(this object obj, string indentation, StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }

            Type objType = obj.GetType();

            stringBuilder.AppendLine($"{indentation ?? string.Empty}{objType.FullName}:");

            string propertiesIndentation = 
                string.IsNullOrEmpty(indentation) ? defaultIndentation : indentation + indentation;

            var properties = 
                objType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach(var property in properties)
            {
                if (property.PropertyType.IsPrimitive)
                {
                    stringBuilder.AppendLine($"{propertiesIndentation}{property.Name} = {property.GetValue(obj)}");
                }
                else
                {
                    stringBuilder.Append(property.GetValue(obj).RenderAsString(propertiesIndentation, stringBuilder));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
