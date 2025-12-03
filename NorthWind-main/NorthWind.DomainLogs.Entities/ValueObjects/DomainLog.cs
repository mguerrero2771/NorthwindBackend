using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.DomainLogs.Entities.ValueObjects
{
    public class DomainLog(string information, string userName)
    {
        public DateTime DateTime => DateTime.Now;
        public string Information => information;
        public string UserName => userName;

    }

}
