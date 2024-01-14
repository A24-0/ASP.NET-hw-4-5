namespace aspdotnet_baza
{
    public interface IEmailSender : IAsyncDisposable {
        public bool IsConnected { get; }
        public bool IsAuthenticated { get; }
        public Task SendEmailAsync(string fromName, string fromEmail, string toName,
            string toEmail, string subject, string body, CancellationToken token);
        public Task ConnectAsync(string host, int port, bool useSsl, CancellationToken token);
        public Task AuthenticateAsync(string email, string password, CancellationToken token);
        public Task DisconnectAsync(bool quit);
    }
}