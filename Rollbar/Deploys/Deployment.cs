using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.Deploys
{
    public class Deployment
    {
        private Deployment()
        {

        }

        public Deployment(string environment, string revision)
            : this(null, environment, revision)
        {

        }

        public Deployment(string writeAccessToken, string environment, string revision)
        {
            this.AccessToken = writeAccessToken;
            this.Environment = environment;
            this.Revision = revision;
        }

        public string AccessToken { get; private set; }
        public string Environment { get; private set; }
        public string Revision { get; private set; }
        public string RollbarUsername { get; set; }
        public string LocalUsername { get; set; }
        public string Comment { get; set; }

    }
}
