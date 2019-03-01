namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Rollbar.Common;
    using Rollbar.DTOs;

    public class HttpRequestPackageDecorator
        : RollbarPackageDecoratorBase
    {
        private readonly HttpRequest _httpRequest;

        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequest httpRequest)
            : this(packageToDecorate, httpRequest, false)
        {
        }

        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequest httpRequest, bool mustApplySynchronously) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._httpRequest = httpRequest;
        }

        protected override void Decorate(Data rollbarData)
        {
            if (this._httpRequest == null)
            {
                return; // nothing to decorate with... 
            }

            if (rollbarData.Request == null)
            {
                rollbarData.Request = new Request();
            }

            rollbarData.Request.Url = this._httpRequest.Host.Value + this._httpRequest.Path;
            rollbarData.Request.QueryString = this._httpRequest.QueryString.Value;
            rollbarData.Request.Params = null;

            rollbarData.Request.Headers = new Dictionary<string, string>(this._httpRequest.Headers.Count);
            foreach (var header in this._httpRequest.Headers)
            {
                rollbarData.Request.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }

            rollbarData.Request.Method = this._httpRequest.Method;

            switch (rollbarData.Request.Method.ToUpper())
            {
                case "POST":
                    this._httpRequest.Body.Seek(0, SeekOrigin.Begin);
                    rollbarData.Request.PostBody = GetBodyAsString(this._httpRequest);
                    break;
            }
        }

        private static string GetBodyAsString(HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (StreamReader reader = new StreamReader(request.Body, encoding))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
