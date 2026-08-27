using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Emailer
{
    public interface ISmtpClientWrapper
    {
        Task ConnectAsync(string host, int port, SecureSocketOptions options,
            CancellationToken ct = default);
        Task AuthenticateAsync(string username, string password,
            CancellationToken ct = default);
        Task SendAsync(MimeMessage message,
            CancellationToken ct = default);
        Task DisconnectAsync(bool quit,
            CancellationToken ct = default);
    }
}
