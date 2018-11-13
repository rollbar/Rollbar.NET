namespace Benchmarker.Common
{
    using BenchmarkDotNet.Attributes;

    [CoreJob(baseline: true)]
    [ClrJob]
    //[CoreRtJob]
    [MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
    //[MonoJob("Mono x86", @"C:\Program Files (x86)\Mono\bin\mono.exe")]
    //[LegacyJitX86Job, LegacyJitX64Job, RyuJitX64Job]
    //[AllStatisticsColumn]
    [MedianColumn, MinColumn, MaxColumn, RankColumn]
    [RPlotExporter]
    //[Config(typeof(BenchmarksConfig))]
    public abstract class BenchmarksBase
    {
        protected readonly System.Exception _smallException = BenchmarkingData.Exception_Small;
        protected readonly System.Exception _mediumException = BenchmarkingData.Exception_Medium;
        protected readonly System.Exception _largeException = BenchmarkingData.Exception_Large;

        protected readonly string _smallMessage = BenchmarkingData.Message_Small;
        protected readonly string _mediumMessage = BenchmarkingData.Message_Medium;
        protected readonly string _largeMessage = BenchmarkingData.Message_Large;

        public BenchmarksBase()
        {
        }

        [Benchmark]
        public abstract void AsyncCriticalException_Small();

        [Benchmark]
        public abstract void AsyncCriticalException_Medium();

        [Benchmark]
        public abstract void AsyncCriticalException_Large();


        [Benchmark]
        public abstract void AsyncInfoMessage_Small();

        [Benchmark]
        public abstract void AsyncInfoMessage_Medium();

        [Benchmark]
        public abstract void AsyncInfoMessage_Large();

    }
}
