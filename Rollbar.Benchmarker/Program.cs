namespace RollbarBenchmarker
{
    using System;
    using BenchmarkDotNet.Running;

    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RollbarLoggerBenchmarks>();
        }
    }
}
