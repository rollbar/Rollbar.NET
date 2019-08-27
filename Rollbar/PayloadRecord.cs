namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using Diagnostics;

    internal class PayloadRecord
    {
        private const string _fieldDelimiter = "|---|";
        private readonly string _destinationUri;
        private readonly string _accessToken;
        private readonly string _payloadContent;

        private PayloadRecord()
        {
        }

        public PayloadRecord(Uri destinationUri, string accessToken, StringContent payloadContent)
        : this(destinationUri.AbsoluteUri, accessToken, payloadContent.ToString())
        {
        }

        public PayloadRecord(string destinationUri, string accessToken, string payloadContent)
        : this(new[] {destinationUri, accessToken, payloadContent, })
        {
        }

        public PayloadRecord(string renderedRecord) 
        : this(renderedRecord.Split(new string[] {_fieldDelimiter, }, StringSplitOptions.None))
        {
        }

        private PayloadRecord(string[] fields)
        {
            Assumption.AssertNotNull(fields, nameof(fields));
            
            const int expectedFieldsCount = 3;
            Assumption.AssertEqual(fields.Length, expectedFieldsCount, nameof(fields.Length));

            int fieldIndex = 0;

            Assumption.AssertNotNullOrWhiteSpace(fields[fieldIndex], $"field-{fieldIndex}");
            this._destinationUri = fields[fieldIndex++];
            Assumption.AssertNotNullOrWhiteSpace(fields[fieldIndex], $"field-{fieldIndex}");
            this._accessToken = fields[fieldIndex++];
            Assumption.AssertNotNullOrWhiteSpace(fields[fieldIndex], $"field-{fieldIndex}");
            this._payloadContent = fields[fieldIndex++];

            Assumption.AssertEqual(fieldIndex, expectedFieldsCount, nameof(fieldIndex));
        }

        public string Render()
        {
            return $"{this._destinationUri}{_fieldDelimiter}{this._accessToken}{_fieldDelimiter}{this._payloadContent}";
        }


    }
}