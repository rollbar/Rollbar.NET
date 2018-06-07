namespace Rollbar.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TelemetryData
    {
        public DateTimeOffset Timestamp { get; }
            = DateTimeOffset.Now;

        public Dictionary<TelemetryAttribute, object> TelemetrySnapshot { get; }
            = new Dictionary<TelemetryAttribute, object>();

        public override string ToString()
        {
            StringBuilder sb = 
                new StringBuilder($"{nameof(TelemetryData)} {this.Timestamp}: {Environment.NewLine}");

            foreach(var key in this.TelemetrySnapshot.Keys)
            {
                object value;
                if (this.TelemetrySnapshot.TryGetValue(key, out value))
                {
                    string unitOfMeasure = string.Empty;
                    switch (key)
                    {
                        case TelemetryAttribute.MachineCpuUtilization:
                        case TelemetryAttribute.ProcessCpuUtilization:
                            break;
                        case TelemetryAttribute.MachineMemoryUtilization:
                        case TelemetryAttribute.ProcessMemoryUtilization:
                            unitOfMeasure = "[bytes]";
                            break;
                    }

                    sb.AppendLine($"   {key}: {value} {unitOfMeasure}");
                }
            }

            return sb.ToString();
        }
    }
}
