namespace hoox.Configuration
{
    public class EmailOptions
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string SendTo { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; } = 80;
    }
}
