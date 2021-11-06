namespace Rollbar.Infrastructure
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// Class PayloadBundle.
    /// </summary>
    internal class PayloadBundle
        : IErrorCollector
    {
        /// <summary>
        /// The time stamp
        /// </summary>
        private readonly DateTime _timeStamp = DateTime.Now;

        /// <summary>
        /// The payload object
        /// </summary>
        private readonly object? _payloadObject;

        /// <summary>
        /// The custom
        /// </summary>
        private readonly IDictionary<string, object?>? _custom;

        /// <summary>
        /// The level
        /// </summary>
        private readonly ErrorLevel _level;

        /// <summary>
        /// The rollbar logger
        /// </summary>
        private readonly IRollbar? _rollbarLogger;

        /// <summary>
        /// The timeout at
        /// </summary>
        private readonly DateTime? _timeoutAt;

        /// <summary>
        /// The signal
        /// </summary>
        private readonly SemaphoreSlim? _signal;

        #region one-time calculated caches

        /// <summary>
        /// The ignorable
        /// </summary>
        private bool? _ignorable;

        /// <summary>
        /// The rollbar package
        /// </summary>
        private IRollbarPackage? _rollbarPackage;

        /// <summary>
        /// The payload
        /// </summary>
        private Payload? _payload;

        #endregion one-time calculated caches

        /// <summary>
        /// Gets the timeout at.
        /// </summary>
        /// <value>The timeout at.</value>
        internal DateTime? TimeoutAt
        {
            get { return this._timeoutAt; }
        }

        /// <summary>
        /// Gets the signal.
        /// </summary>
        /// <value>The signal.</value>
        internal SemaphoreSlim? Signal
        {
            get { return this._signal; }
        }

        /// <summary>
        /// Gets or sets as HTTP content to send.
        /// </summary>
        /// <value>As HTTP content to send.</value>
        internal StringContent? AsHttpContentToSend { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="PayloadBundle"/> class from being created.
        /// </summary>
        private PayloadBundle()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            IRollbarPackage payloadPackage,
            ErrorLevel level
            )
            : this(rollbarLogger, payloadPackage, level, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            IDictionary<string, object?>? custom
            )
            : this(rollbarLogger, payloadPackage, level, custom, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            DateTime? timeoutAt,
            SemaphoreSlim? signal
            )
            : this(rollbarLogger, payloadPackage, level, null, timeoutAt, signal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            IDictionary<string, object?>? custom,
            DateTime? timeoutAt,
            SemaphoreSlim? signal
            )
            : this(rollbarLogger, payloadPackage as object, level, custom, timeoutAt, signal)
        {
            if (payloadPackage != null)
            {
                IRollbarPackage package = ApplyCustomKeyValueDecorator(payloadPackage);
                this._rollbarPackage = package;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            object payloadObject,
            ErrorLevel level
            )
            : this(rollbarLogger, payloadObject, level, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            object payloadObject,
            ErrorLevel level,
            IDictionary<string, object?>? custom
            )
            : this(rollbarLogger, payloadObject, level, custom, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            object payloadObject,
            ErrorLevel level,
            DateTime? timeoutAt,
            SemaphoreSlim? signal
            )
            : this(rollbarLogger, payloadObject, level, null, timeoutAt, signal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbar rollbarLogger,
            object payloadObject,
            ErrorLevel level,
            IDictionary<string, object?>? custom,
            DateTime? timeoutAt,
            SemaphoreSlim? signal
            )
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));
            Assumption.AssertNotNull(payloadObject, nameof(payloadObject));

            this._rollbarLogger = rollbarLogger;
            this._payloadObject = payloadObject;
            this._level = level;
            this._custom = custom;
            this._timeoutAt = timeoutAt;
            this._signal = signal;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PayloadBundle"/> is ignorable.
        /// </summary>
        /// <value><c>true</c> if ignorable; otherwise, <c>false</c>.</value>
        public bool Ignorable
        {
            get
            {
                if (!this._ignorable.HasValue)
                {
                    GetPayload(); // it actually calculates and sets this._ignorable value...
                }
                return this._ignorable!.Value;
            }
        }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <returns>Payload.</returns>
        public Payload? GetPayload()
        {
            if (this._payload == null)
            {
                Data? data = this.GetPayloadData();
                if (data != null)
                {
                    data.Level = this._level;
                    //update the data timestamp from the data creation timestamp to the passed
                    //object-to-log capture timestamp:
                    data.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(this._timeStamp);

                    this._payload = new Payload(this._rollbarLogger?.Config.RollbarDestinationOptions.AccessToken, data);
                    this._payload.PayloadBundle = this;

                    try // payload check-ignore:
                    {
                        if (this._rollbarLogger?.Config.RollbarPayloadManipulationOptions.CheckIgnore != null
                            && this._rollbarLogger.Config.RollbarPayloadManipulationOptions.CheckIgnore.Invoke(this._payload)
                            )
                        {
                            this._ignorable = true;
                            return this._payload;     //shortcut...
                        }
                    }
                    catch (System.Exception exception)
                    {
                        RollbarErrorUtility.Report(
                            this._rollbarLogger, 
                            this._payload, 
                            InternalRollbarError.PayloadCheckIgnoreError, 
                            "While check-ignoring a payload...", 
                            exception,
                            GetPrimaryErrorCollector()
                            );
                    }
                    this._ignorable = false;

                    try // payload transformation:
                    {
                        this._rollbarLogger?.Config.RollbarPayloadManipulationOptions.Transform?.Invoke(this._payload);
                    }
                    catch (System.Exception exception)
                    {
                        RollbarErrorUtility.Report(
                            this._rollbarLogger,
                            this._payload,
                            InternalRollbarError.PayloadTransformError,
                            "While transforming a payload...",
                            exception,
                            GetPrimaryErrorCollector()
                            );
                    }

                    try // payload truncation:
                    {
                        this._rollbarLogger?.Config.RollbarPayloadManipulationOptions.Truncate?.Invoke(this._payload);
                    }
                    catch (System.Exception exception)
                    {
                        RollbarErrorUtility.Report(
                            this._rollbarLogger,
                            this._payload,
                            InternalRollbarError.PayloadTruncationError,
                            "While truncating a payload...",
                            exception,
                            GetPrimaryErrorCollector()
                            );
                    }

                    try
                    {
                        this._payload.Validate();
                    }
                    catch (System.Exception exception)
                    {
                        RollbarErrorUtility.Report(
                            this._rollbarLogger,
                            this._payload,
                            InternalRollbarError.PayloadValidationError,
                            "While validating a payload...",
                            exception,
                            GetPrimaryErrorCollector()
                            );
                    }
                }
            }
            return this._payload;
        }

        /// <summary>
        /// Gets the payload data.
        /// </summary>
        /// <returns>Data.</returns>
        private Data? GetPayloadData()
        {
            Data? data;

            IRollbarPackage? rollbarPackage = GetRollbarPackage();
            Assumption.AssertNotNull(rollbarPackage, nameof(rollbarPackage));

            data = rollbarPackage?.PackageAsRollbarData();
            Assumption.AssertNotNull(data, nameof(data));

            return data;
        }

        /// <summary>
        /// Gets the rollbar package.
        /// </summary>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage? GetRollbarPackage()
        {
            if (this._rollbarPackage == null)
            {
                this._rollbarPackage = CreateRollbarPackage(this._payloadObject);
                if (this._rollbarPackage != null)
                {
                    IErrorCollector? packageErrorCollector = this._rollbarPackage as IErrorCollector;
                    if (packageErrorCollector != null)
                    {
                        foreach (var exception in this._bundleErrorCollector.Exceptions)
                        {
                            packageErrorCollector.Register(exception);
                        }
                    }
                }
            }
            this._rollbarPackage = ApplyConfigPackageDecorator(this._rollbarPackage);
            return this._rollbarPackage;
        }

        /// <summary>
        /// Creates the rollbar package.
        /// </summary>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage? CreateRollbarPackage(object? payloadObject)
        {
            IRollbarPackage? package = null;

            switch (payloadObject)
            {
                case IRollbarPackage packageObject:
                    package = packageObject;
                    package = ApplyCustomKeyValueDecorator(package);
                    return package;
                case Data dataObject:
                    package = new DataPackage(dataObject);
                    package = ApplyCustomKeyValueDecorator(package);
                    return package;
                case Body bodyObject:
                    package = new BodyPackage(this._rollbarLogger?.Config, bodyObject, this._custom);
                    return package;
                case System.Exception exceptionObject:
                    package = new ExceptionPackage(exceptionObject);
                    package = ApplyCustomKeyValueDecorator(package);
                    return package;
                case string messageObject:
                    package = new MessagePackage(messageObject, this._custom);
                    return package;
                case ITraceable traceable:
                    package = new MessagePackage(traceable.TraceAsString(), this._custom);
                    return package;
                default:
                    package = new MessagePackage(payloadObject?.ToString(), this._custom);
                    return package;
            }
        }

        /// <summary>
        /// Applies the custom key value decorator.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage ApplyCustomKeyValueDecorator(IRollbarPackage packageToDecorate)
        {
            if (this._custom == null || this._custom.Count == 0)
            {
                return packageToDecorate; // nothing to decorate with
            }

            return new CustomKeyValuePackageDecorator(packageToDecorate, this._custom);
        }

        /// <summary>
        /// Applies the configuration package decorator.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage? ApplyConfigPackageDecorator(IRollbarPackage? packageToDecorate)
        {
            if (this._rollbarLogger?.Config == null)
            {
                return packageToDecorate; // nothing to decorate with
            }

            return new ConfigAttributesPackageDecorator(packageToDecorate, this._rollbarLogger.Config);
        }

        #region IErrorCollector

        /// <summary>
        /// The bundle's package-related exceptions (if any) that happened during the package lifetime.
        /// </summary>
        public IReadOnlyCollection<System.Exception> Exceptions
        {
            get
            {
                return GetPrimaryErrorCollector().Exceptions;
            }
        }

        /// <summary>
        /// Registers the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Register(System.Exception exception)
        {
            GetPrimaryErrorCollector().Register(exception);
        }

        private IErrorCollector GetPrimaryErrorCollector()
        {
            IErrorCollector? primaryErrorCollector = GetPackageErrorCollectorIfAny();
            if (primaryErrorCollector != null)
            {
                return primaryErrorCollector;
            }

            return this._bundleErrorCollector;
        }

        private IErrorCollector? GetPackageErrorCollectorIfAny()
        {
            IErrorCollector? package = null;
            if (this._rollbarPackage != null)
            {
                package = this._rollbarPackage as IErrorCollector;
            }

            return package;
        }

        /// <summary>
        /// The bundle error collector.
        /// 
        /// NOTE: it is intended to be referenced directly only by the following methods: 
        /// GetRollbarPackage() and GetPrimaryErrorCollector() !!!
        /// </summary>
        private readonly IErrorCollector _bundleErrorCollector = new ErrorCollector();

        #endregion IErrorCollector
    }
}
