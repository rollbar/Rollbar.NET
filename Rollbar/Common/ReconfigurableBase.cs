namespace Rollbar.Common
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Xamarin.iOS.Foundation;

    using Rollbar.Diagnostics;

    /// <summary>
    /// An abstract base for implementing IReconfigurable types.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase" />
    /// Implements the <see cref="System.IEquatable{T}" />
    /// </summary>
    /// <typeparam name="T">A type that supports its reconfiguration.</typeparam>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase" />
    /// <seealso cref="System.IEquatable{T}" />
    /// <seealso cref="Rollbar.Common.IReconfigurable{T}" />
    [Preserve]
    public abstract class ReconfigurableBase<T>
        : ReconfigurableBase
        , IReconfigurable<T>
        , IEquatable<T>
        where T : ReconfigurableBase<T>
    {
        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>
        /// Reconfigured instance.
        /// </returns>
        public virtual T Reconfigure(T likeMe)
        {
            base.Reconfigure(likeMe, this.thisInstanceType);

            return (T) this;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public virtual bool Equals(T? other)
        {
            return base.Equals(other, this.thisInstanceType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as T);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override int GetHashCode()
        {
            return this.CalculateHashCode();
        }
    }


    /// <summary>
    /// An abstract base for implementing IReconfigurable (based on a base type) types.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase" />
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    /// Implements the <see cref="System.IEquatable{TBase}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TBase">The type of the base.</typeparam>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase" />
    /// <seealso cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    /// <seealso cref="System.IEquatable{TBase}" />
    /// <seealso cref="Rollbar.Common.IReconfigurable{T}" />
    public abstract class ReconfigurableBase<T, TBase>
        : ReconfigurableBase
        , IReconfigurable<T, TBase>
        , IEquatable<TBase>
        where T : ReconfigurableBase<T, TBase>, TBase
        where TBase : class, IReconfigurable<TBase, TBase>
    {

        private readonly Type _baseInstanceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconfigurableBase{T, TBase}"/> class.
        /// </summary>
        protected ReconfigurableBase()
        {
            this._baseInstanceType = typeof(TBase);
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>
        /// Reconfigured instance.
        /// </returns>
        public virtual T Reconfigure(TBase likeMe)
        {
            base.Reconfigure(likeMe, this._baseInstanceType);

            return (T) this;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public virtual bool Equals(TBase? other)
        {
            return base.Equals(other, this._baseInstanceType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as TBase);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override int GetHashCode()
        {
            return this.CalculateHashCode();
        }
    }

    /// <summary>
    /// Class ReconfigurableBase.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase" />
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T}" />
    /// Implements the <see cref="System.IEquatable{T}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase" />
    /// <seealso cref="Rollbar.Common.IReconfigurable{T}" />
    /// <seealso cref="System.IEquatable{T}" />
    public abstract class ReconfigurableBase
        : ITraceable
        , IValidatable
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> publicPropertyInfosByType = new();

        /// <summary>
        /// The reconfigurable properties
        /// </summary>
        protected readonly List<ReconfigurableBase> reconfigurableProperties;

        /// <summary>
        /// The this instance type (most specific one).
        /// </summary>
        protected readonly Type thisInstanceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconfigurableBase"/> class.
        /// </summary>
        protected ReconfigurableBase()
        {
            this.thisInstanceType = this.GetType();

            PropertyInfo[] thisInstanceProperyInfos = 
                ReconfigurableBase.ListInstancePublicProperties(this.thisInstanceType);
            List<object?> propertyValues = 
                thisInstanceProperyInfos
                .Select(i => i.GetValue(this))
                .ToList<object?>();
            this.reconfigurableProperties = 
                propertyValues
                .Where(i => i is ReconfigurableBase)
                .Cast<ReconfigurableBase>()
                .ToList();
            foreach(var reconfigurable in this.reconfigurableProperties)
            {
                reconfigurable.Reconfigured += Reconfigurable_Reconfigured;
            }
        }

        private void Reconfigurable_Reconfigured(object? sender, EventArgs e)
        {
            this.OnReconfigured(new EventArgs());
        }

        /// <summary>
        /// Lists the instance's public properties.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>PropertyInfo[].</returns>
        protected static PropertyInfo[] ListInstancePublicProperties(Type objectType)
        {
            if(!publicPropertyInfosByType.TryGetValue(objectType, out var properties))
            {
                properties = ReflectionUtility.GetAllPublicInstanceProperties(objectType);
                publicPropertyInfosByType.TryAdd(objectType,properties);
            }
            return properties;
        }

        /// <summary>
        /// Reconfigures as the specified like-me prototype object.
        /// </summary>
        /// <param name="likeMe">The like-me prototype to reconfigure according to.</param>
        /// <param name="likeMeTypeOfInterest">The like-me's type of interest.</param>
        protected virtual void Reconfigure(object likeMe, Type likeMeTypeOfInterest)
        {
            Assumption.AssertNotNull(likeMe, nameof(likeMe));
            Assumption.AssertTrue(likeMeTypeOfInterest.IsAssignableFrom(this.thisInstanceType), nameof(likeMeTypeOfInterest));

            // In general we could be reconfiguring the destination object 
            // based on a source object that is a subtype of the destination type.
            // Hence, it could contains a subset of the properties available in the destination type.
            // Let's base the reconfiguration process based on that subset:
            PropertyInfo[] properties
                = ReconfigurableBase.ListInstancePublicProperties(likeMeTypeOfInterest);

            foreach (var property in properties)
            {
                // Let's see first if the property value is a Reconfigurable object:
                if (property.GetValue(this) is ReconfigurableBase targetPropertyValue)
                {
                    object? sourcePropertyValue = property.GetValue(likeMe);
                    if (sourcePropertyValue != null)
                    {
                        targetPropertyValue.Reconfigure(sourcePropertyValue, sourcePropertyValue.GetType());
                    }
                    continue;
                }

                // For non-Reconfigurable properties, let's clone the property value:
                if (property.CanWrite)
                {
                    property.SetValue(this, property.GetValue(likeMe));
                }
                else
                {
                    // This case handles situations when the reconfiguration source object has read-only
                    // property but the destination object could have an equivalent read-write property:
                    PropertyInfo? destinationProperty
                        = ReconfigurableBase.ListInstancePublicProperties(this.thisInstanceType)
                            .SingleOrDefault(p => p.Name == property.Name);
                    if (destinationProperty != null && destinationProperty.CanWrite)
                    {
                        destinationProperty.SetValue(this, property.GetValue(likeMe));
                    }
                }
            }

            this.OnReconfigured(new EventArgs());
        }

        /// <summary>
        /// Checks if this instance equals the specified other object in terms of the public properties' values of the other's type.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <param name="otherType">Type of the other.</param>
        /// <returns><c>true</c> if not equal, <c>false</c> otherwise.</returns>
        protected bool Equals(object? other, Type otherType)
        {
            if (other == null)
            {
                return false;
            }

            if (this == other)
            {
                return true;
            }

            if (!otherType.IsAssignableFrom(this.thisInstanceType))
            {
                return false;
            }

            PropertyInfo[] properties
                = ReconfigurableBase.ListInstancePublicProperties(otherType);

            return this.HaveEqualPropertyValues(this, other, properties);
        }

        private bool HaveEqualPropertyValues(object? left, object? right, IEnumerable<PropertyInfo> properties)
        {
            if (left == null && right == null)
            {
                return true;
            }
            if (!(left != null && right != null))
            {
                return false;
            }
            if (left == right)
            {
                return true;
            }

            foreach (var property in properties)
            {
                //compare this instance property values to other's:
                object? leftPropertyValue = property.GetValue(left);
                object? rightPropertyValue = property.GetValue(right);

                if (leftPropertyValue == rightPropertyValue)
                {
                    continue; // same property value...
                }
                if ((leftPropertyValue == null) && (rightPropertyValue == null))
                {
                    continue; // both are null...
                }
                if ((leftPropertyValue != null) && leftPropertyValue.Equals(rightPropertyValue))
                {
                    continue; // are equal according to values .Equals(other) method...
                }
                if (!((leftPropertyValue != null) && (rightPropertyValue != null)))
                {
                    return false; // one side null another not...
                }
                if ((property.PropertyType.IsPrimitive || property.PropertyType == typeof(string) || property.PropertyType == typeof(Guid))
                    && !object.Equals(leftPropertyValue, rightPropertyValue)
                    )
                {
                    return false;
                }
                if (property.PropertyType.GetInterface(typeof(IEnumerable).FullName!) != null)
                {
#pragma warning disable IDE0019 // Use pattern matching
                    var leftCollection = leftPropertyValue as ICollection;
                    var rightCollection = rightPropertyValue as ICollection;
#pragma warning restore IDE0019 // Use pattern matching
                    if (leftCollection != null && rightCollection != null && leftCollection.Count != rightCollection.Count)
                    {
                        return false;
                    }
                    var leftEnumeration = leftPropertyValue as IEnumerable;
                    var rightEnumeration = rightPropertyValue as IEnumerable;
                    if(leftEnumeration == null && rightEnumeration == null)
                    {
                        return true;
                    }
                    else if(leftEnumeration == null || rightEnumeration == null)
                    {
                        return false;
                    }
                    foreach (var leftItem in leftEnumeration)
                    {
                        bool hasMatchingItem = false;
                        foreach (var rightItem in rightEnumeration)
                        {
                            Type? leftItemType = leftItem?.GetType();
                            Type? rightItemType = rightItem?.GetType();

                            if (leftItemType != rightItemType)
                            {
                                continue;
                            }

                            if (leftItemType != null && (leftItemType.IsPrimitive || leftItemType == typeof(string) || leftItemType == typeof(Guid)))
                            {
                                if (leftItem != null && leftItem.Equals(rightItem))
                                {
                                    hasMatchingItem = true;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if(rightItem != null)
                            {
                                var itemProperties = ListInstancePublicProperties(rightItem.GetType());
                                if(HaveEqualPropertyValues(leftItem, rightItem, itemProperties))
                                {
                                    hasMatchingItem = true;
                                    break;
                                }
                            }
                        }
                        if (!hasMatchingItem)
                        {
                            return false;
                        }
                    }
                    continue; //foreach (var property in properties)
                }
                if (!HaveEqualPropertyValues(leftPropertyValue, rightPropertyValue, ListInstancePublicProperties(property.PropertyType)))
                {
                    return false;
                }
            }

            // if we made it all the way here, it means all the property values matched:
            return true;
        }

        /// <summary>
        /// Occurs when this instance reconfigured.
        /// </summary>
        public event EventHandler? Reconfigured;

        /// <summary>
        /// Raises the <see cref="E:Reconfigured" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnReconfigured(EventArgs e)
        {
            Reconfigured?.Invoke(this,e);
        }

        /// <summary>
        /// Calculates the hash code.
        /// </summary>
        /// <returns>
        /// System.Int32.
        /// </returns>
        protected int CalculateHashCode()
        {
#if NET || NETCOREAPP3_1
            return this.TraceAsString().GetHashCode(StringComparison.OrdinalIgnoreCase);
#else
            return this.TraceAsString().GetHashCode();
#endif
        }

        #region ITraceable

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string TraceAsString()
        {
            return this.TraceAsString(string.Empty);
        }

        /// <summary>
        /// Traces as a string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        public virtual string TraceAsString(string indent)
        {
            StringBuilder sb = new();
            sb.AppendLine("{" + this.thisInstanceType.FullName + "}");

            PropertyInfo[] properties
                = ReconfigurableBase.ListInstancePublicProperties(this.thisInstanceType);

            foreach(var property in properties)
            {
                object? propertyValue = property.GetValue(this);
                string? propertyValueTrace = "<null>";
                switch(propertyValue)
                {
                    case null:
                        break;
                    case ICollection collection:
                        StringBuilder collectionTrace = new();
                        collectionTrace.AppendLine("[");
                        foreach(var item in collection)
                        {
                            switch(item)
                            {
                                case ITraceable traceable:
                                    collectionTrace.AppendLine($"{indent}    {traceable.TraceAsString()},");
                                    break;
                                default:
                                    collectionTrace.AppendLine($"{indent}    {item},");
                                    break;
                            }
                        }
                        collectionTrace.Append($"{indent}  ]");
                        propertyValueTrace = collectionTrace.ToString();
                        break;
                    case ITraceable traceable:
                        propertyValueTrace = traceable.TraceAsString(indent + "  ");
                        int newLineSuffixIndex = propertyValueTrace.LastIndexOf(Environment.NewLine);
                        if(newLineSuffixIndex >= 0)
                        {
                            propertyValueTrace = propertyValueTrace.Remove(newLineSuffixIndex, Environment.NewLine.Length);
                        }
                        break;
                    default:
                        propertyValueTrace = propertyValue.ToString();
                        break;
                }
                sb.AppendLine($"{indent}  {property.Name}: {propertyValueTrace}");
            }

            return sb.ToString();
        }

        #endregion ITraceable

        #region IValidatable

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules.</returns>
        public IReadOnlyCollection<ValidationResult> Validate()
        {
            var validator = this.GetValidator();

            if(validator != null)
            {
                var failedValidations = validator.Validate(this);

                return failedValidations;
            }
            else
            {
                return ArrayUtility.GetEmptyArray<ValidationResult>();
            }

        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public abstract Validator? GetValidator();

        #endregion IValidatable
    }
}
