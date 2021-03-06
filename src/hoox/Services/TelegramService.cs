using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using hoox.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace hoox.Services
{
    public class TelegramService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly IOptions<TelegramOptions> _telegramOptions;

        public TelegramService(ILogger<TelegramService> logger, IOptions<TelegramOptions> telegramOptions)
        {
            _logger = logger;
            _telegramOptions = telegramOptions;
        }
        public async Task SendTelegram(JObject content)
        {

            Func<ChatId, string, Task> execute = null;

            if (_telegramOptions.Value.UseProxy)
            {
                _logger.LogInformation("Using proxy configuration....");
                IWebProxy proxy = WebRequest.GetSystemWebProxy();
                if (_telegramOptions.Value.ProxyHost != null)
                {
                    proxy = new WebProxy(new Uri($"{_telegramOptions.Value.ProxyHost}:{_telegramOptions.Value.ProxyPort}", UriKind.Absolute))
                    {
                        UseDefaultCredentials = false
                    };
                }

                if (_telegramOptions.Value.UseClassicRestClient)
                {
                    execute = async (chatId, message) =>
                    {
                        _logger.LogInformation("Using CLASSIC REST client...");
                        var httpClient = new HttpClient(new HttpClientHandler
                        {
                            UseProxy = true,
                            Proxy = proxy
                        });

                        var request = await httpClient.PostAsync(
                            $"https://api.telegram.org/bot{_telegramOptions.Value.ApiKey}/sendMessage",
                            new StringContent(JsonConvert.SerializeObject(new
                            {
                                chat_id = chatId.ToString(),
                                text = message,
                                parse_mode = "MarkdownV2"
                            }), Encoding.UTF8, "application/json"));
                    };
                }
                else
                {
                    
                    execute = async (chatId, message) =>
                    {
                        _logger.LogInformation("Using CHAT BOT client...");
                        var bot = new Telegram.Bot.TelegramBotClient(_telegramOptions.Value.ApiKey, proxy);
                        await bot.SendTextMessageAsync(chatId, message, ParseMode.MarkdownV2);
                    };
                }
            }
            else
            {
                _logger.LogInformation("NOT using a proxy...");
                var httpClient = new HttpClient(new HttpClientHandler
                {
                    UseProxy = false,
                    Proxy = null,
                    Credentials = null
                });
                if (_telegramOptions.Value.UseClassicRestClient)
                {
                    execute = async (chatId, message) =>
                    {
                        _logger.LogInformation("Using CLASSIC REST client...");
                        var request = await httpClient.PostAsync(
                            $"https://api.telegram.org/bot{_telegramOptions.Value.ApiKey}/sendMessage",
                            new StringContent(JsonConvert.SerializeObject(new
                            {
                                chat_id = chatId.ToString(),
                                text = message,
                                parse_mode = "MarkdownV2"
                            }),Encoding.UTF8, "application/json"));
                    };
                }
                else
                {
                    _logger.LogInformation("Using CHAT BOT client...");
                    var bot = new Telegram.Bot.TelegramBotClient(_telegramOptions.Value.ApiKey, httpClient);
                    execute = async (chatId, message) =>
                    {
                        await bot.SendTextMessageAsync(chatId, message, ParseMode.MarkdownV2);
                    };
                }
            }

            StringBuilder body = new StringBuilder();
            var subject = content.ContainsKey("title")
                ? content["title"]?.ToString()
                : "Proxied notification message";

            body.AppendLine($"*{SanitizeMarkdownString(subject)}*");
            body.AppendLine();
            body.AppendLine($"__{SanitizeMarkdownString(content.ContainsKey("message") ? content["message"]?.ToString() : "")}__");
            body.AppendLine();
            body.AppendLine($@"```json
{content.ToString(Formatting.Indented)}
```");

            var chatId = new ChatId(_telegramOptions.Value.ChatId);
            await execute(chatId, body.ToString());
        }

        public static string SanitizeMarkdownString(string markdown)
        {
            var tokens = new char[]{ '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            foreach (var token in tokens)
            {
                markdown = markdown.Replace(token.ToString(), $"\\{token}");
            }

            return markdown;
        }
    }
}