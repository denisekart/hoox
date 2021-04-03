namespace hoox.Configuration
{
    public class TelegramOptions
    {
        public string ApiKey { get; set; }
        public string ChatId { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; } = 80;
    }
}