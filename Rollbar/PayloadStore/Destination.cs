namespace Rollbar.PayloadStore {

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    public class Destination 
    {
        public Guid ID { get; set; }
        public string Endpoint { get; }
        public string AccessToken { get; }

        private ICollection<PayloadRecord> PayloadRecords { get; }
    }
}
