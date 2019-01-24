namespace Rollbar.Common
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xamarin.iOS.Foundation;

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
            base.Reconfigure(likeMe, this._thisInstanceType);

            return (T) this;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(T other)
        {
            return base.Equals(other, this._thisInstanceType);
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
    {

        private readonly Type _baseInstanceType = null;

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

            return (T)this;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(TBase other)
        {
            return base.Equals(other, this._baseInstanceType);
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
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> publicPropertyInfosByType
            = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// The this instance type (most specific one).
        /// </summary>
        protected readonly Type _thisInstanceType = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconfigurableBase"/> class.
        /// </summary>
        protected ReconfigurableBase()
        {
            this._thisInstanceType = this.GetType();
        }

        /// <summary>
        /// Lists the instance's public properties.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>PropertyInfo[].</returns>
        protected static PropertyInfo[] ListInstancePublicProperties(Type objectType)
        {
            PropertyInfo[] properties = null;
            if (!publicPropertyInfosByType.TryGetValue(objectType, out properties))
            {
                properties = ReflectionUtility.GetAllPublicInstanceProperties(objectType);
                publicPropertyInfosByType.TryAdd(objectType, properties);
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
            Assumption.AssertTrue(likeMeTypeOfInterest.IsAssignableFrom(this._thisInstanceType), nameof(likeMeTypeOfInterest));

            // In general we could be reconfiguring the destination object 
            // based on a source object that is a subtype of the destination type.
            // Hence, it could contains a subset of the properties available in the destination type.
            // Let's base the reconfiguration process based on that subset:
            PropertyInfo[] properties
                = ReconfigurableBase.ListInstancePublicProperties(likeMeTypeOfInterest);

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    property.SetValue(this, property.GetValue(likeMe));
                }
                else
                {
                    // This case handles situations when the reconfiguration source object has read-only
                    // property but the destination object could have an equivalent read-write property:
                    var destinationProperty
                        = ReconfigurableBase.ListInstancePublicProperties(this._thisInstanceType)
                            .SingleOrDefault(p => p.Name == property.Name);
                    if (destinationProperty.CanWrite)
                    {
                        destinationProperty.SetValue(this, property.GetValue(likeMe));
                    }
                }
            }

            OnReconfigured(new EventArgs());

            return;
        }

        /// <summary>
        /// Checks if this instance equals the specified other object in terms of the public properties' values of the other's type.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <param name="otherType">Type of the other.</param>
        /// <returns><c>true</c> if not equal, <c>false</c> otherwise.</returns>
        protected bool Equals(object other, Type otherType)
        {
            if (other == null)
            {
                return false;
            }

            if (this == other)
            {
                return true;
            }

            if (!otherType.IsAssignableFrom(this._thisInstanceType))
            {
                return false;
            }

            PropertyInfo[] properties
                = ReconfigurableBase.ListInstancePublicProperties(otherType);

            return this.HaveEqualPropertyValues(this, other, properties);
        }

        private bool HaveEqualPropertyValues(object left, object right, IEnumerable<PropertyInfo> properties)
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
                object leftPropertyValue = property.GetValue(left);
                object rightPropertyValue = property.GetValue(right);

                if (leftPropertyValue == rightPropertyValue)
                {
                    continue; // same property value...
                }
                if ((leftPropertyValue == null) && (rightPropertyValue == null))
                {
                    continue; // both are null...
                }
                if (leftPropertyValue.Equals(rightPropertyValue))
                {
                    continue; // are equal according to values .Equals(other) method...
                }
                //ReconfigurableBase reconfigurableProperty = leftPropertyValue as ReconfigurableBase;
                //if ((reconfigurableProperty != null) && reconfigurableProperty.Equals(rightPropertyValue, property.PropertyType))
                //{
                //    continue; // equal according to ReconfigurableBase.Equals(object other, Type otherType)...
                //}
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
                if (property.PropertyType.GetInterface(typeof(IEnumerable).FullName) != null)
                {
                    var leftCollection = leftPropertyValue as ICollection;
                    var rightCollection = rightPropertyValue as ICollection;
                    if (leftCollection != null && rightCollection != null && leftCollection.Count != rightCollection.Count)
                    {
                        return false;
                    }
                    var leftEnumeration = leftPropertyValue as IEnumerable;
                    var rightEnumeration = rightPropertyValue as IEnumerable;
                    foreach (var leftItem in leftEnumeration)
                    {
                        var lProperties = ListInstancePublicProperties(leftItem.GetType());
                        bool hasMatchingItem = false;
                        foreach (var rightItem in rightEnumeration)
                        {
                            Type leftItemType = leftItem.GetType();
                            Type rightItemType = rightItem.GetType();

                            if (leftItemType != rightItemType)
                            {
                                continue;
                            }

                            if (leftItemType.IsPrimitive || leftItemType == typeof(string) || leftItemType == typeof(Guid))
                            {
                                if (leftItem.Equals(rightItem))
                                {
                                    hasMatchingItem = true;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            var itemProperties = ListInstancePublicProperties(rightItem.GetType());
                            if (HaveEqualPropertyValues(leftItem, rightItem, itemProperties))
                            {
                                hasMatchingItem = true;
                                break;
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
        public event EventHandler Reconfigured;

        /// <summary>
        /// Raises the <see cref="E:Reconfigured" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnReconfigured(EventArgs e)
        {
            EventHandler handler = Reconfigured;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

}
