namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection;

    public static class ObjectExtension
    {
        private const string defaultIndentation = "  ";

        public static string RenderAsString(this object obj, string indentation = null)
        {
            return RenderAsString(obj, indentation, new StringBuilder());
        }

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
