using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Emailer
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly SmtpClient _client = new SmtpClient();
        public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancelationToken)
            => _client.ConnectAsync(host, port, options, cancelationToken);
        public Task AuthenticateAsync(string username, string password, CancellationToken cancelationToken)
            => _client.AuthenticateAsync(username, password, cancelationToken);
        public Task SendAsync(MimeMessage message, CancellationToken cancelationToken)
            => _client.SendAsync(message, cancelationToken);
        public Task DisconnectAsync(bool quit, CancellationToken cancelationToken)
            => _client.DisconnectAsync(quit, cancelationToken);
    }
}
