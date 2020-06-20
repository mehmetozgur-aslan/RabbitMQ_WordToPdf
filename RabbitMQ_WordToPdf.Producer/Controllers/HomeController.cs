using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ_WordToPdf.Producer.Models;

namespace RabbitMQ_WordToPdf.Producer.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult WordToPdf()
        {
            return View();
        }

        [HttpPost]
        public IActionResult WordToPdf(WordToPdfPackage wordToPdf)
        {
            var factory = new ConnectionFactory();

            factory.Uri = new Uri(configuration["ConnectionStrings:RabbitMQCloudString"]);

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("convert-exchange", ExchangeType.Direct, true, false, null);

                    channel.QueueDeclare(queue: "File", durable: true, exclusive: false, autoDelete: false, arguments: null);

                    channel.QueueBind("File", "convert-exchange", "WordToPdf");

                    MessageWordToPdf messageWordToPdf = new MessageWordToPdf();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        wordToPdf.File.CopyTo(ms);
                        messageWordToPdf.WordByte = ms.ToArray();
                    }
                    messageWordToPdf.Email = wordToPdf.Email;
                    messageWordToPdf.FileName = Path.GetFileNameWithoutExtension(wordToPdf.File.FileName);

                    string serializeMessage = JsonConvert.SerializeObject(messageWordToPdf);

                    byte[] byteMessage = Encoding.UTF8.GetBytes(serializeMessage);

                    // Mesaj sağlama alma
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish("convert-exchange", routingKey: "WordToPdf", basicProperties: properties, body: byteMessage);

                    ViewBag.result = "Word dosyanız pdf dosyasına dönüştürüldükten sonra size email olarak gönderilecektir.";

                    return View();
                }
            }
        }
    }
}
