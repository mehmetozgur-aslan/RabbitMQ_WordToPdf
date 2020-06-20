using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ_WordToPdf.Producer.Models
{
    public class WordToPdfPackage
    {
        public string Email { get; set; }
        public IFormFile File { get; set; }
    }
}
