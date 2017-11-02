namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Client 
        : ExtendableDtoBase
    {
        protected Client(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        public static class ReservedProperties
        {
        }
    }
}
