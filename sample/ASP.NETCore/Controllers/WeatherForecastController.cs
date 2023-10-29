using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace ASP.NETCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ScopedLogContext _requestLogContext;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ScopedLogContext requestLogContext)
        {
            _logger = logger;
            _requestLogContext = requestLogContext;
        }

        [HttpGet]
        public WeatherForecast GetTomorrowForecast()
        {
            var forecast =  new WeatherForecast
            {
                Date = DateTime.Now.AddDays(1),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
            _requestLogContext.PushProperty("TomorrowForecast", forecast, destructureObjects: true);
            if (forecast.TemperatureC < -10)
                throw new Exception("It's going to be freezing cold!");
            return forecast;
        }
    }
}