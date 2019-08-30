namespace Rollbar.PayloadStore {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class PayloadRecord
    {
        public Guid ID { get; set; }

        public long Timestamp { get; }
        public string PayloadJson { get; }

        public Destination Destination { get; }
        public Guid DestinationID { get; }

    }
}
