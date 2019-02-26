namespace Rollbar
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
    {
        /// <summary>
        /// The time stamp
        /// </summary>
        private readonly DateTime _timeStamp = DateTime.UtcNow;

        /// <summary>
        /// The payload object
        /// </summary>
        private readonly object _payloadObject;
        /// <summary>
        /// The custom
        /// </summary>
        private readonly IDictionary<string, object> _custom;
        /// <summary>
        /// The level
        /// </summary>
        private readonly ErrorLevel _level;
        /// <summary>
        /// The rollbar configuration
        /// </summary>
        private readonly IRollbarConfig _rollbarConfig;
        /// <summary>
        /// The timeout at
        /// </summary>
        private readonly DateTime? _timeoutAt;
        /// <summary>
        /// The signal
        /// </summary>
        private readonly SemaphoreSlim _signal;

        // one-time calculated caches:
        //****************************
        /// <summary>
        /// The ignorable
        /// </summary>
        private bool? _ignorable;
        /// <summary>
        /// The rollbar package
        /// </summary>
        private IRollbarPackage _rollbarPackage;
        /// <summary>
        /// The payload
        /// </summary>
        private Payload _payload;
        /// <summary>
        /// As HTTP content to send
        /// </summary>
        private StringContent _asHttpContentToSend;

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
        internal SemaphoreSlim Signal
        {
            get { return this._signal; }
        }

        /// <summary>
        /// Gets or sets as HTTP content to send.
        /// </summary>
        /// <value>As HTTP content to send.</value>
        internal StringContent AsHttpContentToSend
        {
            get
            {
                return this._asHttpContentToSend;
            }
            set
            {
                this._asHttpContentToSend = value;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="PayloadBundle"/> class from being created.
        /// </summary>
        private PayloadBundle()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level
            )
            : this(rollbarConfig, payloadPackage, level, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            IDictionary<string, object> custom
            )
            : this(rollbarConfig, payloadPackage, level, custom, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            DateTime? timeoutAt,
            SemaphoreSlim signal
            )
            : this(rollbarConfig, payloadPackage, level, null, timeoutAt, signal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadPackage">The payload package.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            IDictionary<string, object> custom,
            DateTime? timeoutAt,
            SemaphoreSlim signal
            )
            : this(rollbarConfig, payloadPackage as object, level, custom, timeoutAt, signal)
        {
            if (payloadPackage != null)
            {
                IRollbarPackage package = ApplyCustomKeyValueDecorator(payloadPackage);
                //package = ApplyConfigPackageDecorator(package);
                this._rollbarPackage = package;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level
            )
            : this(rollbarConfig, payloadObject, level, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level,
            IDictionary<string, object> custom
            )
            : this(rollbarConfig, payloadObject, level, custom, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level,
            DateTime? timeoutAt,
            SemaphoreSlim signal
            )
            : this(rollbarConfig, payloadObject, level, null, timeoutAt, signal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadBundle"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="payloadObject">The payload object.</param>
        /// <param name="level">The level.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level,
            IDictionary<string, object> custom,
            DateTime? timeoutAt,
            SemaphoreSlim signal
            )
        {
            Assumption.AssertNotNull(rollbarConfig, nameof(rollbarConfig));
            Assumption.AssertNotNull(payloadObject, nameof(payloadObject));

            this._rollbarConfig = rollbarConfig;
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
                return this._ignorable.Value;
            }
        }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <returns>Payload.</returns>
        public Payload GetPayload()
        {
            if (this._payload == null)
            {
                Data data = this.GetPayloadData();
                if (data != null)
                {
                    // we are already setting environment within the ConfigAttributesPackageDecorator:
                    //data.Environment = this._rollbarConfig?.Environment;

                    data.Level = this._level;
                    //update the data timestamp from the data creation timestamp to the passed
                    //object-to-log capture timestamp:
                    data.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(this._timeStamp);

                    this._payload = new Payload(this._rollbarConfig.AccessToken, data);

                    try // payload check-ignore:
                    {
                        if (this._rollbarConfig.CheckIgnore != null
                            && this._rollbarConfig.CheckIgnore.Invoke(this._payload)
                            )
                        {
                            this._ignorable = true;
                            return this._payload;     //shortcut...
                        }
                    }
                    catch (System.Exception ex)
                    {
                        //TODO: substitute following:
                        //OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  check-ignoring a payload..."));
                    }
                    this._ignorable = false;

                    try // payload transformation:
                    {
                        this._rollbarConfig.Transform?.Invoke(this._payload);
                    }
                    catch (System.Exception ex)
                    {
                        //TODO: substitute following:
                        //OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  transforming a payload..."));
                    }

                    try // payload truncation:
                    {
                        this._rollbarConfig.Truncate?.Invoke(this._payload);
                    }
                    catch (System.Exception ex)
                    {
                        //TODO: substitute following:
                        //OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  truncating a payload..."));
                    }

                    this._payload.Validate();
                    //TODO: we may want to handle/report nicely if the validation failed...
                }
            }
            return this._payload;
        }

        /// <summary>
        /// Gets the payload data.
        /// </summary>
        /// <returns>Data.</returns>
        private Data GetPayloadData()
        {
            Data data;

            IRollbarPackage rollbarPackage = GetRollbarPackage();
            Assumption.AssertNotNull(rollbarPackage, nameof(rollbarPackage));

            //if (rollbarPackage.RollbarData != null)
            //{
            //    data = rollbarPackage.RollbarData;
            //}
            //else
            {
                data = rollbarPackage.PackageAsRollbarData();
            }
            Assumption.AssertNotNull(data, nameof(data));

            return data;
        }

        /// <summary>
        /// Gets the rollbar package.
        /// </summary>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage GetRollbarPackage()
        {
            IRollbarPackage rollbarPackage = CreateRollbarPackage();
            rollbarPackage = ApplyConfigPackageDecorator(rollbarPackage);
            return rollbarPackage;
        }

        /// <summary>
        /// Creates the rollbar package.
        /// </summary>
        /// <returns>IRollbarPackage.</returns>
        private IRollbarPackage CreateRollbarPackage()
        {
            if (this._rollbarPackage != null)
            {
                return this._rollbarPackage;
            }

            switch (this._payloadObject)
            {
                case IRollbarPackage packageObject:
                    this._rollbarPackage = packageObject;
                    this._rollbarPackage = ApplyCustomKeyValueDecorator(this._rollbarPackage);
                    return this._rollbarPackage;
                case Data dataObject:
                    this._rollbarPackage = new DataPackage(dataObject);
                    this._rollbarPackage = ApplyCustomKeyValueDecorator(this._rollbarPackage);
                    return this._rollbarPackage;
                case Body bodyObject:
                    this._rollbarPackage = new BodyPackage(this._rollbarConfig, bodyObject, this._custom);
                    //this._rollbarPackage = ApplyCustomKeyValueDecorator(this._rollbarPackage);
                    return this._rollbarPackage;
                case System.Exception exceptionObject:
                    this._rollbarPackage = new ExceptionPackage(exceptionObject);
                    this._rollbarPackage = ApplyCustomKeyValueDecorator(this._rollbarPackage);
                    return this._rollbarPackage;
                case string messageObject:
                    this._rollbarPackage = new MessagePackage(messageObject, this._custom);
                    return this._rollbarPackage;
                case ITraceable traceable:
                    this._rollbarPackage = new MessagePackage(traceable.TraceAsString(), this._custom);
                    return this._rollbarPackage;
                default:
                    this._rollbarPackage = new MessagePackage(this._payloadObject.ToString(), this._custom);
                    return this._rollbarPackage;
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
        private IRollbarPackage ApplyConfigPackageDecorator(IRollbarPackage packageToDecorate)
        {
            if (this._rollbarConfig == null)
            {
                return packageToDecorate; // nothing to decorate with
            }

            return new ConfigAttributesPackageDecorator(packageToDecorate, this._rollbarConfig);

        }
    }
}
