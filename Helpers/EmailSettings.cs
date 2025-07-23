namespace SistemaBodega.Helpers
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class EmailProvidersSettings
    {
        public string DefaultProvider { get; set; }
        public Dictionary<string, SmtpSettings> Providers { get; set; }
    }
}
