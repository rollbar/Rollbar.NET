namespace Rollbar.Benchmarker
{
    using System;
    using BenchmarkDotNet.Running;

    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RollbarLoggerBenchmark>();
        }
    }
}
