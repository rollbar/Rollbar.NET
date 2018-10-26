namespace Rollbar.Benchmarker
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using Rollbar;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    [CoreJob(baseline: true)]
    [ClrJob]
    //[CoreRtJob]
    [MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
    //[MonoJob("Mono x86", @"C:\Program Files (x86)\Mono\bin\mono.exe")]
    //[LegacyJitX86Job, LegacyJitX64Job, RyuJitX64Job]
    //[AllStatisticsColumn]
    [MedianColumn, MinColumn, MaxColumn, RankColumn]
    [RPlotExporter]
    //[Config(typeof(RollbarLoggerBenchmarkConfig))]
    public class RollbarLoggerBenchmark
    {
        private readonly IRollbar _rollbar = RollbarLoggerBenchmark.ProvideRollbar();

        private readonly System.Exception _smallException = RollbarLoggerBenchmark.ProvideException(PayloadSize.Small);
        private readonly System.Exception _mediumException = RollbarLoggerBenchmark.ProvideException(PayloadSize.Medium);
        private readonly System.Exception _largeException = RollbarLoggerBenchmark.ProvideException(PayloadSize.Large);

        private readonly string _smallMessage = RollbarLoggerBenchmark.ProvideMessage(PayloadSize.Small);
        private readonly string _mediumMessage = RollbarLoggerBenchmark.ProvideMessage(PayloadSize.Medium);
        private readonly string _largeMessage = RollbarLoggerBenchmark.ProvideMessage(PayloadSize.Large);

        public RollbarLoggerBenchmark()
        {
        }

        [Benchmark]
        public ILogger AsyncCriticalException_Small() => this._rollbar.Critical(this._smallException);

        [Benchmark]
        public ILogger AsyncCriticalException_Medium() => this._rollbar.Critical(this._mediumException);

        [Benchmark]
        public ILogger AsyncCriticalException_Large() => this._rollbar.Critical(this._largeException);


        [Benchmark]
        public ILogger AsyncInfoMessage_Small() => this._rollbar.Info(this._smallMessage);

        [Benchmark]
        public ILogger AsyncInfoMessage_Medium() => this._rollbar.Info(this._mediumMessage);

        [Benchmark]
        public ILogger AsynInfoMessage_Large() => this._rollbar.Info(this._largeMessage);



        private static string ProvideMessage(PayloadSize payloadSize)
        {
            string smallMessage = @"Small message 1234 56789! ";
            StringBuilder result = new StringBuilder(smallMessage);

            int multiplier = 1;
            switch (payloadSize)
            {
                case PayloadSize.Small:
                    multiplier = 1;
                    break;
                case PayloadSize.Medium:
                    multiplier = 1 * Constants.SmallToMediumMessageMultiplier;
                    break;
                case PayloadSize.Large:
                    multiplier = 1 * Constants.SmallToMediumMessageMultiplier * Constants.MediumToLargeMessageMultiplier;
                    break;
                default:
                    Assumption.FailValidation("Unexpected value!", nameof(payloadSize));
                    break;
            }

            int counter = 0;
            while (++counter < multiplier)
            {
                result.AppendLine(smallMessage);
            }

            return result.ToString();
        }

        private static System.Exception ProvideException(PayloadSize payloadSize)
        {
            const int totalExceptionFramesBaseline = 3;
            int multiplier = 1;
            switch (payloadSize)
            {
                case PayloadSize.Small:
                    multiplier = 1;
                    break;
                case PayloadSize.Medium:
                    multiplier = 1 * Constants.SmallToMediumExceptionCallStackDepthMultiplier;
                    break;
                case PayloadSize.Large:
                    multiplier = 1 * Constants.SmallToMediumExceptionCallStackDepthMultiplier * Constants.MediumToLargeExceptionCallStackDepthMultiplier;
                    break;
                default:
                    Assumption.FailValidation("Unexpected value!", nameof(payloadSize));
                    break;
            }

            return ExceptionSimulator.GetExceptionWith(multiplier * totalExceptionFramesBaseline);
        }

        private static IRollbar ProvideRollbar()
        {
            IRollbarConfig loggerConfig = ProvideRollbarConfig();

            // Let's give things change to stabilize: 
            //Thread.Sleep(TimeSpan.FromSeconds(2));

            return RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        private static IRollbarConfig ProvideRollbarConfig()
        {
            RollbarConfig loggerConfig = new RollbarConfig(Constants.RollbarAccessToken)
            {
                Environment = Constants.RollbarEnvironment,
            };

            Person person = new Person("RBM");
            person.Email = "benchmarker@RollbarBenchmarks.org";
            person.UserName = "RollbarBenchmarker";
            loggerConfig.Person = person;

            return loggerConfig;
        }

    }
}
