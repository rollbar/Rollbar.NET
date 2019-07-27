namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Rollbar.Common;
    using Rollbar.DTOs;

    public class HttpResponsePackageDecorator
        : RollbarPackageDecoratorBase
    {
        private readonly HttpResponse _httpResponse;

        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponse httpResponse)
            : this(packageToDecorate, httpResponse, false)
        {

        }

        public HttpResponsePackageDecorator(IRollbarPackage packageToDecorate, HttpResponse httpResponse, bool mustApplySynchronously) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._httpResponse = httpResponse;
        }

        protected override void Decorate(Data rollbarData)
        {
            if (this._httpResponse == null)
            {
                return; // nothing to decorate with... 
            }

            if (rollbarData.Response == null)
            {
                rollbarData.Response = new Response();
            }

            rollbarData.Response.StatusCode = this._httpResponse.StatusCode;

            rollbarData.Response.Headers = new Dictionary<string, string>(this._httpResponse.Headers.Count);
            foreach (var header in this._httpResponse.Headers)
            {
                rollbarData.Response.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }


            throw new NotImplementedException();
        }
    }
}
