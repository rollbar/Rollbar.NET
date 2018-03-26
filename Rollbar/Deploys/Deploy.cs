namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

  //"result": {
  //  "id": 8387647,
  //  "project_id": 67454,
  //  "user_id": null,
  //  "environment": "unit-tests",
  //  "revision": "99909a3a5a3dd4363f414161f340b582bb2e4161",
  //  "local_username": null,
  //  "comment": null,
  //  "start_time": 1522095147,
  //  "finish_time": 1522095148
  //}

    public class Deploy
        : IDeployment
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DeployID { get; set; }

        [JsonProperty("project_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ProjectID { get; set; }

        [JsonProperty("revision", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Revision { get; set; }

        [JsonProperty("environment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Environment { get; set; }

        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty("local_username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocalUsername { get; set; }

        [JsonProperty("user_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RollbarUsername { get; set; }

        [JsonProperty("start_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long StartTime { get; set; }

        [JsonProperty("finish_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long EndTime { get; set; }
    }
}
