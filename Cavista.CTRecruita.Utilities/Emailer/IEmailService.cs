using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Emailer
{
    public interface IEmailService
    {
        Task SendMailAsync(string subject, string body, string receiver, bool hasAttachment, List<string>? attachmentLocs = null, Stream? attachStream = null, string? attachmentName = null, string? reply = null, List<string>? bcc = null, List<string>? cc = null);
    }
}
