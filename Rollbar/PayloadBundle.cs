namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PayloadBundle
    {
        private readonly DateTime _timeStamp = DateTime.UtcNow;

        private readonly object _payloadObject;
        private readonly IDictionary<string, object> _custom;
        private readonly ErrorLevel _level;
        private readonly IRollbarConfig _rollbarConfig;
        private readonly DateTime? _timeoutAt;
        private readonly SemaphoreSlim _signal;

        // one-time calculated caches:
        //****************************
        private bool? _ignorable;
        private IRollbarPackage _rollbarPackage;
        private Payload _payload;
        private StringContent _asHttpContentToSend;

        internal DateTime? TimeoutAt
        {
            get { return this._timeoutAt; }
        }

        internal SemaphoreSlim Signal
        {
            get { return this._signal; }
        }

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

        private PayloadBundle()
        {

        }

        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level
            )
            : this(rollbarConfig, payloadPackage, level, null, null, null)
        {
        }

        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            IRollbarPackage payloadPackage,
            ErrorLevel level,
            IDictionary<string, object> custom
            )
            : this(rollbarConfig, payloadPackage, level, custom, null, null)
        {
        }

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
            this._rollbarPackage = payloadPackage;
        }

        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level
            )
            : this(rollbarConfig, payloadObject, level, null, null, null)
        {
        }

        public PayloadBundle(
            IRollbarConfig rollbarConfig,
            object payloadObject,
            ErrorLevel level,
            IDictionary<string, object> custom
            )
            : this(rollbarConfig, payloadObject, level, custom, null, null)
        {
        }

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

        private Data GetPayloadData()
        {
            Data data;

            IRollbarPackage rollbarPackage = GetRollbarPackage();
            Assumption.AssertNotNull(rollbarPackage, nameof(rollbarPackage));

            if (rollbarPackage.RollbarData != null)
            {
                data = rollbarPackage.RollbarData;
            }
            else
            {
                data = rollbarPackage.PackageAsRollbarData();
            }
            Assumption.AssertNotNull(data, nameof(data));

            return data;
        }

        private IRollbarPackage GetRollbarPackage()
        {
            IRollbarPackage rollbarPackage = CreateRollbarPackage();
            rollbarPackage = ApplyConfigPackageDecorator(rollbarPackage);
            return rollbarPackage;
        }

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

        private IRollbarPackage ApplyCustomKeyValueDecorator(IRollbarPackage packageToDecorate)
        {
            if (this._custom == null || this._custom.Count == 0)
            {
                return packageToDecorate; // nothing to decorate with
            }

            return new CustomKeyValuePackageDecorator(packageToDecorate, this._custom);
        }

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
