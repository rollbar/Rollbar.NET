[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar.Telemetry
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class TelemetryCollector
    {
        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static TelemetryCollector Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TelemetryCollector"/> class from being created.
        /// </summary>
        private TelemetryCollector()
        {
            this.StartAutocollection();
        }

        private sealed class NestedSingleInstance
        {
            static NestedSingleInstance()
            {
            }

            internal static readonly TelemetryCollector Instance =
                new TelemetryCollector();
        }

        #endregion singleton implementation

        public TelemetryConfig Config { get; } = new TelemetryConfig();

        public TelemetryQueue TelemetryQueue { get; } = new TelemetryQueue();

        private void CollectThisProcessTelemetry(TelemetryData telemetryData)
        {
            Process currentProcess = null;
            if (TelemetrySettings.ProcessCpuUtilization == (this.Config.TelemetrySettings & TelemetrySettings.ProcessCpuUtilization)
                || TelemetrySettings.ProcessMemoryUtilization == (this.Config.TelemetrySettings & TelemetrySettings.ProcessMemoryUtilization)
                )
            {
                currentProcess = Process.GetCurrentProcess();
            }
            if (currentProcess != null)
            {
                telemetryData.TelemetrySnapshot[TelemetryAttribute.ProcessCpuUtilization] = GetCpuUsage();// currentProcess.TotalProcessorTime;
                telemetryData.TelemetrySnapshot[TelemetryAttribute.ProcessMemoryUtilization] = currentProcess.WorkingSet64;
            }

            //we can also try using performance counters (but it would not work for .Net Core)
            // https://gavindraper.com/2011/03/01/retrieving-accurate-cpu-usage-in-c/

        }

        private PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public int GetCpuUsage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (int)_cpuCounter.NextValue();
            }

            var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

            return 0;
        }

        private void CollectMachineTelemetry(TelemetryData telemetryData)
        {
            //NOTE: getting properties of some of other processes causes access denied exceptions.
            //      Making the current process run with admin privileges may solve it but that would
            //      require users to do the same for their apps hosting the SDK.
            //      I guess we can not count on that...
            TimeSpan? totalProcessorTime = null;
            long? totalWorkingSet = null;
            if (TelemetrySettings.MachineCpuUtilization == (this.Config.TelemetrySettings & TelemetrySettings.MachineCpuUtilization))
            {
                totalProcessorTime = TimeSpan.Zero;
            }
            if (TelemetrySettings.MachineMemoryUtilization == (this.Config.TelemetrySettings & TelemetrySettings.MachineMemoryUtilization))
            {
                totalWorkingSet = 0;
            }
            if (totalProcessorTime.HasValue || totalWorkingSet.HasValue)
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (totalProcessorTime.HasValue)
                    {
                        totalProcessorTime += process.TotalProcessorTime;
                    }
                    if (totalWorkingSet.HasValue)
                    {
                        totalWorkingSet += process.WorkingSet64;
                    }
                }
            }
            if (totalProcessorTime.HasValue)
            {
                telemetryData.TelemetrySnapshot[TelemetryAttribute.MachineCpuUtilization] = totalProcessorTime.Value;
            }
            if (totalWorkingSet.HasValue)
            {
                telemetryData.TelemetrySnapshot[TelemetryAttribute.MachineMemoryUtilization] = totalWorkingSet.Value;
            }
        }

        private void CollectTelemetry()
        {
            var telemetryData = new TelemetryData();

            CollectThisProcessTelemetry(telemetryData);
            //CollectMachineTelemetry(telemetryData);

            this.TelemetryQueue.Enqueue(telemetryData);
        }

        private void KeepCollectingTelemetry(object data)
        {
            CancellationToken cancellationToken = (CancellationToken)data;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.CollectTelemetry();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (System.Threading.ThreadAbortException tae)
                {
                    return;
                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
                    //TODO: do we want to direct the exception 
                    //      to some kind of Rollbar notifier maintenance "access token"?
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if (cancellationToken.IsCancellationRequested)
                    break;

                Thread.Sleep(this.Config.TelemetryCollectionInterval);
            }

            CompleteProcessing();
        }

        private Thread _telemetryThread = null;
        private CancellationTokenSource _cancellationTokenSource = null;
        private readonly object _syncRoot = new object();

        public void StartAutocollection()
        {
            if (!this.Config.TelemetryEnabled)
            {
                return; // no need to start at all...
            }
            if (this.Config.TelemetrySettings == TelemetrySettings.None)
            {
                return; // nothing really to collect...
            }

            // let's resync with relevant config settings: 
            this.TelemetryQueue.QueueDepth = this.Config.TelemetryQueueDepth;

            lock (_syncRoot)
            {
                if (this._telemetryThread == null)
                {
                    this._telemetryThread = new Thread(new ParameterizedThreadStart(this.KeepCollectingTelemetry))
                    {
                        IsBackground = true,
                        Name = "Rollbar Telemetry Thread"
                    };

                    this._cancellationTokenSource = new CancellationTokenSource();
                    this._telemetryThread.Start(_cancellationTokenSource.Token);
                }
            }
        }

        private void CompleteProcessing()
        {
            if (this._cancellationTokenSource == null)
            {
                return;
            }

            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
            this._cancellationTokenSource.Dispose();
            this._cancellationTokenSource = null;
            this._telemetryThread = null;
        }

        public void StopAutocollection(bool immediate)
        {
            lock(_syncRoot)
            {
                if (this._cancellationTokenSource == null)
                {
                    return;
                }

                this._cancellationTokenSource.Cancel();
                if (immediate && this._telemetryThread != null)
                {
                    this._telemetryThread.Join(TimeSpan.FromSeconds(60));
                    CompleteProcessing();
                }
            }
        }

        public bool IsAutocollecting
        {
            get
            {
                lock(this._syncRoot)
                {
                    return !(this._cancellationTokenSource == null || this._cancellationTokenSource.IsCancellationRequested);
                }
            }
        }

    }
}
