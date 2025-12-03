using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Backend.SmtpGateways.Options
{
    public class SmtpOptions
    {
        public const string SectionKey = nameof(SmtpOptions);
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpHostPort { get; set; } = 25;
        public string SmtpUserName { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string AdministratorEmail { get; set; } = string.Empty;
    }
}
