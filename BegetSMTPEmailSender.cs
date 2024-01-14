using MailKit.Net.Smtp;
using MimeKit;

namespace aspdotnet_baza {
    public class BegetSMTPEmailSender : IEmailSender {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
        private bool disposed;
        public bool IsConnected => _smtpClient.IsConnected;
        public bool IsAuthenticated => _smtpClient.IsAuthenticated;

        public BegetSMTPEmailSender(ILogger logger){
            disposed = false;
            _logger = logger;
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
        }
        public async Task ConnectAsync(string host, int port, bool useSsl, CancellationToken token){
            if (_smtpClient.IsConnected == false){
                await semaphoreSlim.WaitAsync(token);
                await _smtpClient.ConnectAsync(host, port, useSsl, token);
                semaphoreSlim.Release();
            }
            else throw new InvalidOperationException("BegetSMTPEmailSender already connected");
        }
        public async Task AuthenticateAsync(string email, string password, CancellationToken token){
            if (_smtpClient.IsAuthenticated == false){
                await semaphoreSlim.WaitAsync(token);
                await _smtpClient.AuthenticateAsync(email, password, token);
                semaphoreSlim.Release();
            }
            else throw new InvalidOperationException("BegetSMTPEmailSender already authenticated");
        }
        public async Task DisconnectAsync(bool quit){
            if (_smtpClient.IsConnected == true){
                await semaphoreSlim.WaitAsync();
                await _smtpClient.DisconnectAsync(quit);
                semaphoreSlim.Release();
            }
            else throw new InvalidOperationException("BegetSMTPEmailSender already disconnected");
        }
        public async ValueTask DisposeAsync(){ await DisposeAsync(true); GC.SuppressFinalize(this); }
        private async Task DisposeAsync(bool disposing){
            if (disposed == true) return;
            if (disposing){
                if (_smtpClient.IsConnected == true){
                    await semaphoreSlim.WaitAsync();
                    await _smtpClient.DisconnectAsync(true);
                    semaphoreSlim.Release();
                }
                _smtpClient.Dispose();
            }
            disposed = true;
        }
        public async Task SendEmailAsync(string fromName, string fromEmail, string toName,
            string toEmail, string subject, string body, CancellationToken token){
            if (_smtpClient.IsConnected == false)
                throw new InvalidOperationException("BegetSMTPEmailSender isn't connected");
            if (_smtpClient.IsAuthenticated == false)
                throw new InvalidOperationException("BegetSMTPEmailSender isn't authenticated");
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            mimeMessage.To.Add(new MailboxAddress(toName, toEmail));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain){ Text = body };
            try {
                await semaphoreSlim.WaitAsync(token);
                var response = await _smtpClient.SendAsync(mimeMessage, token);
                _logger.LogInformation($"smpt server reply: {response}");
                semaphoreSlim.Release(); 
            }
            catch (Exception) { _logger.LogInformation($"smtp server reply: message not delivered"); }
        }
    }
}

