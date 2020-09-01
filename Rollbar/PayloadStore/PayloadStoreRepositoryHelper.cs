using System;
using System.IO;
using System.Reflection;

namespace Rollbar.PayloadStore
{
    public static class PayloadStoreRepositoryHelper
    {
        const string ASSEMBLY = "Rollbar.OfflinePersistence.dll";

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

        public const string TYPE_LOAD_ERROR_MESSAGE = "To enable offline persistence, you must provide the package 'Rollbar.OfflinePersistence' as a dependency of your project, so the Rollbar.OfflinePersistence.dll is available.";
        static IPayloadStoreRepository RaiseTypeLoadException() => throw new TypeLoadException(TYPE_LOAD_ERROR_MESSAGE);
    }
}
