namespace Rollbar.PayloadStore {

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    public class Destination 
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Endpoint { get; set; }
        public string AccessToken { get; set; }

        public ICollection<PayloadRecord> PayloadRecords { get; set; }
    }
}
