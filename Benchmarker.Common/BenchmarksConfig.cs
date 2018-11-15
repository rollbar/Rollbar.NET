namespace Benchmarker.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Horology;
    using BenchmarkDotNet.Jobs;

    public class BenchmarksConfig : ManualConfig
    {
        public BenchmarksConfig()
        {
            //Add(new Job1(), new Job2());
            //Add(new Column1(), new Column2());
            //Add(new Exporter1(), new Exporter2());
            //Add(new Logger1(), new Logger2());
            //Add(new Diagnoser1(), new Diagnoser2());
            //Add(new Analyser1(), new Analyser2());
            //Add(new Filter1(), new Filter2());

            Job[] jobs = new Job[]
            {
                //Job.Clr,
                //Job.Core,
                //Job.CoreRT,
                Job.Mono,
            };

            Runtime[] runtimes = new Runtime[]
            {
                //Runtime.Clr,
                //Runtime.Core,
                //Runtime.CoreRT,
                //Runtime.Mono,
                new MonoRuntime("Mono x64", @"C:\Program Files\Mono\bin\mono.exe"),
            };

            Platform[] platforms = new Platform[]
            {
                //Platform.AnyCpu,
                Platform.X64,
                //Platform.X86,
            };

            Jit[] jits = new Jit[]
            {
                Jit.Default,
                //Jit.LegacyJit,
                //Jit.Llvm,
                //Jit.RyuJit,
            };

            foreach (var platform in platforms)
            {
                foreach (var runtime in runtimes)
                {
                    foreach (var jit in jits)
                    {
                        foreach(var job in jobs)
                        {
                            Add(
                                job
                                .With(platform)
                                .With(jit)
                                .With(runtime)
                                .WithLaunchCount(1)
                                .WithMinIterationCount(100)
                                .WithIterationCount(100)
                                .WithMaxIterationCount(110)
                                .WithIterationTime(TimeInterval.Millisecond * 50)
                                .WithMaxRelativeError(0.01)
                                .WithId(platform + "-" + runtime + "-" + jit + "-" + job.Id));
                        }
                    }
                }
            }

        }
    }
}
