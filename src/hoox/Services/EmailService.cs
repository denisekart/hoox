using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Oauth2.v2;
using hoox.Configuration;
using MailKit.Net.Proxy;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace hoox.Services
{
    public class EmailService
    {
        private readonly IOptions<EmailOptions> _emailOptions;

        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions;
        }

        public async Task SendMail(JObject content)
        {
            var message = new MimeMessage();
            var from = new MailboxAddress(_emailOptions.Value.FromName, _emailOptions.Value.FromAddress);
            message.From.Add(from);
            message.ReplyTo.Add(from);

            foreach (var recepient in _emailOptions.Value.SendTo.Split(",", StringSplitOptions.RemoveEmptyEntries))
            {
                var to = new MailboxAddress(recepient, recepient);
                message.To.Add(to);
            }

            message.Subject = content.ContainsKey("title")
                ? content["title"]?.ToString()
                : "Email notification proxied message";

            StringBuilder body = new StringBuilder();
            body.AppendLine(content.ContainsKey("message") ? content["message"]?.ToString() : "");
            body.AppendLine();
            body.AppendLine(content.ToString());
            body.AppendLine();

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = body.ToString();

            message.Body = bodyBuilder.ToMessageBody();

            await SendSmtp(message);
        }

        private async Task SendGmail(MimeMessage message)
        {
            var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Encode(message.ToString())
            };

            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("xxxxx")
            {
                User = _emailOptions.Value.SmtpUser,
                Scopes = new List<string>
                {
                    "https://mail.google.com/"
                }
            }.FromPrivateKey("-----BEGIN PRIVATE KEY-----\nxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n-----END PRIVATE KEY-----\n"));
            var success = await credential.RequestAccessTokenAsync(CancellationToken.None);

            var gmailService = new GmailService(new Oauth2Service.Initializer()
            {
                HttpClientInitializer = credential
            });
            var request = gmailService.Users.Messages.Send(gmailMessage, "me");

            await request.ExecuteAsync();
        }

        private static string Encode(string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);

            return System.Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private async Task SendSmtp(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                if (_emailOptions.Value.UseProxy)
                {
                    if (_emailOptions.Value.ProxyHost != null)
                    {
                        client.ProxyClient =
                            new HttpProxyClient(_emailOptions.Value.ProxyHost, _emailOptions.Value.ProxyPort);
                    }
                }
                else
                {
                    client.ProxyClient = null;
                }

                await client.ConnectAsync(_emailOptions.Value.SmtpHost, _emailOptions.Value.SmtpPort, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_emailOptions.Value.SmtpUser, _emailOptions.Value.SmtpPassword);
                await client.SendAsync(FormatOptions.Default, message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
