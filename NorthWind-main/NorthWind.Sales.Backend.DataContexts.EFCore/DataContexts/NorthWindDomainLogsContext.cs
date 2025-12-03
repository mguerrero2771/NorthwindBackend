using NorthWind.Sales.Backend.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts
{
    internal class NorthWindDomainLogsContext(IOptions<DBOptions> dbOptions)
 : DbContext
    {
        public DbSet<DomainLog> DomainLogs { get; set; }
        protected override void OnConfiguring(
       DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
            dbOptions.Value.DomainLogsConnectionString);
        }
    }

}
