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
        /// <param name="indentation">The indentation.</param>
        /// <returns>System.String.</returns>
        public static string RenderAsString(this object obj, string indentation = null)
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

            if (string.IsNullOrEmpty(indentation))
            {
                indentation = defaultIndentation;
            }
            else
            {
                indentation += indentation;
            }

            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach(var property in properties)
            {
                if (property.PropertyType.IsPrimitive)
                {
                    stringBuilder.AppendLine($"{indentation}{property.Name} = {property.GetValue(obj)}");
                }
                else
                {
                    stringBuilder.Append(property.GetValue(obj).RenderAsString(indentation, stringBuilder));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
