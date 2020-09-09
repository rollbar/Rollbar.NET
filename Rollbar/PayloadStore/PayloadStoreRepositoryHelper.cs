namespace Rollbar.PayloadStore
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Class PayloadStoreRepositoryHelper.
    /// </summary>
    public static class PayloadStoreRepositoryHelper
    {
        /// <summary>
        /// The offline persistence store access assembly
        /// </summary>
        const string ASSEMBLY = "Rollbar.OfflinePersistence.dll";

        /// <summary>
        /// Creates the payload store repository.
        /// </summary>
        /// <returns>IPayloadStoreRepository.</returns>
        public static IPayloadStoreRepository CreatePayloadStoreRepository()
        {
            if (!File.Exists(ASSEMBLY))
                return RaiseTypeLoadException();


            var assembly = Assembly.LoadFrom(ASSEMBLY);
            if (assembly == null)
                return RaiseTypeLoadException();

            var type = assembly.GetType("Rollbar.OfflinePersistence.PayloadStoreRepository", false);
            if (type == null)
                return RaiseTypeLoadException();

            var ctor = type.GetConstructor(new Type[0]);
            if (ctor == null)
                return RaiseTypeLoadException();

            return (IPayloadStoreRepository)ctor.Invoke(new object[0]); ;
        }

        /// <summary>
        /// The type load error message
        /// </summary>
        public const string TYPE_LOAD_ERROR_MESSAGE = 
            "To enable offline persistence, you must provide the package 'Rollbar.OfflinePersistence' as a dependency of your project, so the Rollbar.OfflinePersistence.dll is available.";

        /// <summary>
        /// Raises the type load exception.
        /// </summary>
        /// <returns>IPayloadStoreRepository.</returns>
        /// <exception cref="TypeLoadException"></exception>
        static IPayloadStoreRepository RaiseTypeLoadException() => throw new TypeLoadException(TYPE_LOAD_ERROR_MESSAGE);
    }
}
