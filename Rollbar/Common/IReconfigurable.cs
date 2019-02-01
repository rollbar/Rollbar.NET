namespace Rollbar.Common
{
    using System;

    /// <summary>
    /// Defines generic IReconfigurable interface.
    /// 
    /// Any type that supports its own reconfiguration based on a provided original
    /// configuration should implement this interface.
    /// </summary>
    /// <typeparam name="T">A type that supports its reconfiguration.</typeparam>
    public interface IReconfigurable<T>
    {
        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">
        /// The pre-configured instance to be cloned in terms of its configuration/settings.
        /// </param>
        /// <returns>Reconfigured instance.</returns>
        T Reconfigure(T likeMe);

        /// <summary>
        /// Occurs when this instance reconfigured.
        /// </summary>
        event EventHandler Reconfigured;
    }

    /// <summary>
    /// Defines generic IReconfigurable interface.
    /// 
    /// Any type that supports its own reconfiguration based on a provided original
    /// base configuration should implement this interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TBase">The type of the base.</typeparam>
    public interface IReconfigurable<out T, in TBase>
        where T : TBase
    {
        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">
        /// The pre-configured instance to be cloned in terms of its configuration/settings.
        /// </param>
        /// <returns>Reconfigured instance.</returns>
        T Reconfigure(TBase likeMe);

        /// <summary>
        /// Occurs when this instance reconfigured.
        /// </summary>
        event EventHandler Reconfigured;
    }

}
