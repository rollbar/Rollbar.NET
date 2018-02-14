namespace Sample.AspNetCore2.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            Rollbar.RollbarConfig loggerConfig = new Rollbar.RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };


            var rollbar = Rollbar.RollbarFactory.CreateNew().Configure(loggerConfig);
            Rollbar.RollbarQueueController.Instance.InternalEvent += Rollbar_InternalEvent;
            rollbar.AsBlockingLogger(TimeSpan.FromSeconds(60)).Info("Hahah!");


            Rollbar.DTOs.Data data = new Rollbar.DTOs.Data(
                config: rollbar.Config,
                body: new Rollbar.DTOs.Body(new Rollbar.DTOs.Message("WebApiCore GET handler is called.")),
                custom: null,
                request: new Rollbar.DTOs.Request(null, this.Request)
                );
            rollbar.Log(data);

            //// Let's simulate an unhandled exception:
            throw new Exception("AspNetCore2.WebApi sample: Unhandled exception within the ValueController");

            return new string[] { "value1", "value2" };
        }

        private void Rollbar_InternalEvent(object sender, Rollbar.RollbarEventArgs e)
        {
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
