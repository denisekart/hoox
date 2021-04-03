using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoox.Controllers
{
    [Route("log")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        [HttpGet, HttpPut, HttpPatch, HttpPost]
        [Route("webhook"), Route("hook")]
        public async Task<IActionResult> Log()
        {
            StringBuilder sb = new StringBuilder($@"
Triggered a webhook at path '{Request.Path}' using the http method '{Request.Method}'.
Request headers:
{string.Join(Environment.NewLine, Request.Headers.Select(h => $"{h.Key}={h.Value}"))}
");

            switch (Request.Method.ToLower())
            {
                case "post":
                case "put":
                case "patch":
                    {
                        sb.AppendLine("Body:");

                        if (Request.ContentLength > 0)
                        {
                            using StreamReader stream = new StreamReader(Request.Body);
                            sb.AppendLine(await stream.ReadToEndAsync());
                        }
                        else
                        {
                            sb.AppendLine("Null");
                        }

                        break;
                    }
                case "get":
                    {

                        sb.AppendLine("Query:");
                        sb.AppendLine(Request.QueryString.HasValue ? Request.QueryString.Value : "Null");
                        break;
                    }
            }


            _logger.LogInformation(sb.ToString());

            return Ok();
        }
    }
}
