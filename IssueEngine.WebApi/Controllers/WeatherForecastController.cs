using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace IssueEngine.WebApi.Controllers
{
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
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("work")]
        public IActionResult DoWork()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            return Ok();
        }

        [HttpGet("trigger2")]
        public IActionResult TriggerTheIssueEngine()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "svc_issueengine",
                Password = "1337",
                Port = 5672,
                VirtualHost = "IssueEngine"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string message = "Bien reçu monsieur !";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "IssueEngine.Exchange.Out",
                                     routingKey: "NxPro",
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            return Ok();
        }

    }
}
