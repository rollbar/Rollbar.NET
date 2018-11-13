namespace RollbarBenchmarker
{
    using BenchmarkDotNet.Attributes;
    using Benchmarker.Common;
    using Rollbar;
    using Rollbar.DTOs;

    public class RollbarLoggerBenchmarks
        : BenchmarksBase
    {
        private readonly IRollbar _rollbar = RollbarLoggerBenchmarks.ProvideRollbar();

        public RollbarLoggerBenchmarks()
        {
        }

        [Benchmark]
        public override void AsyncCriticalException_Small() => this._rollbar.Critical(this._smallException);

        [Benchmark]
        public override void AsyncCriticalException_Medium() => this._rollbar.Critical(this._mediumException);

        [Benchmark]
        public override void AsyncCriticalException_Large() => this._rollbar.Critical(this._largeException);


        [Benchmark]
        public override void AsyncInfoMessage_Small() => this._rollbar.Info(this._smallMessage);

        [Benchmark]
        public override void AsyncInfoMessage_Medium() => this._rollbar.Info(this._mediumMessage);

        [Benchmark]
        public override void AsyncInfoMessage_Large() => this._rollbar.Info(this._largeMessage);


        private static IRollbar ProvideRollbar()
        {
            IRollbarConfig loggerConfig = ProvideRollbarConfig();

            // Let's give things change to stabilize: 
            //Thread.Sleep(TimeSpan.FromSeconds(2));

            return RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        private static IRollbarConfig ProvideRollbarConfig()
        {
            RollbarConfig loggerConfig = new RollbarConfig(RollbarConstants.RollbarAccessToken)
            {
                Environment = RollbarConstants.RollbarEnvironment,
            };

            Person person = new Person("RBM");
            person.Email = "benchmarker@RollbarBenchmarks.org";
            person.UserName = "RollbarBenchmarker";
            loggerConfig.Person = person;

            return loggerConfig;
        }

    }
}
