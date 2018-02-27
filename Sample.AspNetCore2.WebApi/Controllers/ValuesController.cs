namespace Sample.AspNetCore2.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Route("api/[controller]")]
    public class ValuesController 
        : Controller
    {
        public readonly ILogger _logger = null;

        public ValuesController(ILogger<ValuesController> logger)
        {
            this._logger = logger;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            this._logger.LogInformation(nameof(ValuesController) + ".Get() is called...");

            //let's use a scoped instance of the notifier here:
            //const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            //const string rollbarEnvironment = "RollbarNetSamples";
            //Rollbar.RollbarConfig loggerConfig = new Rollbar.RollbarConfig(rollbarAccessToken)
            //{
            //    Environment = rollbarEnvironment,
            //};
            //using (var rollbar = Rollbar.RollbarFactory.CreateNew().Configure(loggerConfig))
            //{
            //    Rollbar.RollbarQueueController.Instance.InternalEvent += Rollbar_InternalEvent;
            //    rollbar.AsBlockingLogger(TimeSpan.FromSeconds(60)).Info("Just a basic test...");
            //}

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
