using Hangfire;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Emailer
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public EmailService(IConfiguration configuration, ISmtpClientWrapper smtpClientWrapper, IBackgroundJobClient backgroundJobClient)
        {
            _configuration = configuration;
            _smtpClientWrapper = smtpClientWrapper;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task SendMailAsync(string subject, string body, string receiver, bool hasAttachment, List<string>? attachmentLocs = null, Stream attachStream = null, string attachmentName = null, string replyto = null, List<string> bcc = null, List<string> cc = null)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["Smtp:FromEmail"]));
            email.To.Add(MailboxAddress.Parse(receiver));

            if (!string.IsNullOrWhiteSpace(replyto))
            {
                email.ReplyTo.Add(MailboxAddress.Parse(replyto));
            }

            if (bcc?.Any() ?? false)
            {
                foreach (var item in bcc)
                {
                    email.Bcc.Add(MailboxAddress.Parse(item));
                }
            }

            if (cc?.Any() ?? false)
            {
                foreach (var item in cc)
                {
                    email.Cc.Add(MailboxAddress.Parse(item));
                }
            }

            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            await _smtpClientWrapper.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), SecureSocketOptions.StartTls);
            await _smtpClientWrapper.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
            await _smtpClientWrapper.SendAsync(email);
            await _smtpClientWrapper.DisconnectAsync(true);
        }
    }
}
