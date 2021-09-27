namespace Sample.AspNetCore.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            this._logger.LogInformation(nameof(WeatherForecastController) + $" is created.");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            this._logger.LogInformation(nameof(WeatherForecastController) + $".Get() is called.");

            var rng = new Random();
            int id = 0;
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                ID = ++id,
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
        }

        [HttpGet("{id}")]
        public ActionResult<WeatherForecast> GetByID(int id)
        {
            this._logger.LogInformation(nameof(WeatherForecastController) + $".GetByID(int id) is called.");

            var rng = new Random();
            var record = new WeatherForecast
            {
                ID = id,
                Date = DateTime.Now.AddDays(id),
                TemperatureC = (2 * id),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };

            var result = Ok(record);
            this._logger.LogInformation(nameof(WeatherForecastController) + $".GetByID(...) executed with result: {result}.");

            return result;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            this._logger.LogInformation(nameof(WeatherForecastController) + $".Post(...) is called with body.");
            //throw new Exception("AspNetCore2.WebApi sample: Unhandled exception within the ValueController.Post(...)");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
