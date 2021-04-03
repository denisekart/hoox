using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using hoox.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace hoox.Controllers
{
    [Route("grafana")]
    public class GrafanaProxyController : ControllerBase
    {
        private readonly ILogger<GrafanaProxyController> _logger;
        private readonly EmailService _emailService;
        private readonly TelegramService _telegramService;

        public GrafanaProxyController(ILogger<GrafanaProxyController> logger, EmailService emailService, TelegramService telegramService)
        {
            _logger = logger;
            _emailService = emailService;
            _telegramService = telegramService;
        }

        [HttpPut, HttpPost, Route("mail.json")]
        public async Task<IActionResult> MailJson([FromBody] JsonElement content)
        {
            await _emailService.SendMail(JObject.Parse(content.GetRawText()));
            
            return Ok();
        }

        [HttpPut, HttpPost, Route("telegram.json")]
        public async Task<IActionResult> TelegramJson([FromBody] JsonElement content)
        {
            await _telegramService.SendTelegram(JObject.Parse(content.GetRawText()));

            return Ok();
        }
    }
}
