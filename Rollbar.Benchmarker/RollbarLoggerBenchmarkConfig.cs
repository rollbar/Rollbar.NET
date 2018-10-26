namespace Rollbar.Benchmarker
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BenchmarkDotNet.Configs;

    public class RollbarLoggerBenchmarkConfig : ManualConfig
    {
        public RollbarLoggerBenchmarkConfig()
        {
            //Add(new Job1(), new Job2());
            //Add(new Column1(), new Column2());
            //Add(new Exporter1(), new Exporter2());
            //Add(new Logger1(), new Logger2());
            //Add(new Diagnoser1(), new Diagnoser2());
            //Add(new Analyser1(), new Analyser2());
            //Add(new Filter1(), new Filter2());
        }
    }
}
