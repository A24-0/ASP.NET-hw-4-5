using System.Threading;

namespace aspdotnet_baza {
    public class BackgroundEmailMemoryService : BackgroundService {
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCheck _memoryCheck;
        private readonly TimeSpan _timeout;
        private readonly IConfiguration _configuration;
        public BackgroundEmailMemoryService(IEmailSender emailSender, TimeSpan timeout,
            IMemoryCheck memoryCheck, IConfiguration configuration){
            _emailSender = emailSender;
            _timeout = timeout;
            _memoryCheck = memoryCheck;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken){
            await _emailSender.ConnectAsync(
                _configuration["BegetSMTPLog_inData:Host"] ?? throw new ArgumentException("Host"),
                int.Parse(_configuration["BegetSMTPLog_inData:Port"] ?? throw new ArgumentException("Port")),
                false,
                stoppingToken);
            await _emailSender.AuthenticateAsync(
                _configuration["BegetSMTPLog_inData:Login"] ?? throw new ArgumentException("Login"),
                _configuration["BegetSMTPLog_inData:Password"] ?? throw new ArgumentException("Password"),
                stoppingToken);
            while (!stoppingToken.IsCancellationRequested){
                try{
                    await _emailSender.SendEmailAsync(
                        "anonymous", "asp2022pd011@rodion-m.ru", "AG", "desu99@bk.ru",
                        "Memory", _memoryCheck.MemoryUsedByApp(), stoppingToken);
                    await Task.Delay(_timeout, stoppingToken);
                }
                catch (Exception){
                    await _emailSender.DisconnectAsync(true);
                    await _emailSender.DisposeAsync();
                }
            }
        }
    }
}
