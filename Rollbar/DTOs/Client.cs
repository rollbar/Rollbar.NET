namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    public class Client 
        : ExtendableDtoBase
    {
        public Client()
            : this(null)
        {
        }

        public Client(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        public static class ReservedProperties
        {
        }
    }
}
