namespace Rollbar.DesignPatterns.Construction.FactoryMethod
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface IFactoryMethod for creating products of certain type.
    /// </summary>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    public interface IFactoryMethod<out TProduct>
    {
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>TProduct.</returns>
        TProduct Create();
    }

    /// <summary>
    /// Interface IFactoryMethod
    /// </summary>
    /// <typeparam name="TProduct">The type of the product.</typeparam>
    /// <typeparam name="THint">The type of the hint to be used to create the product.</typeparam>
    public interface IFactoryMethod<out TProduct, in THint>
    {
        /// <summary>
        /// Creates the specified hint.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <returns>TProduct.</returns>
        TProduct Create(THint hint);
    }
}
