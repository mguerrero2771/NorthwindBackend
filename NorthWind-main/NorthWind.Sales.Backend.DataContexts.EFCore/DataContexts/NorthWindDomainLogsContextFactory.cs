using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
internal class NorthWindDomainLogsContextFactory :
 IDesignTimeDbContextFactory<NorthWindDomainLogsContext>
{
    public NorthWindDomainLogsContext CreateDbContext(string[] args)
    {
        IOptions<DBOptions> DbOptions =
        Microsoft.Extensions.Options.Options.Create(
        new DBOptions
        {
           /* DomainLogsConnectionString =
        "Data Source=SEBAS;Initial Catalog=NorthWindLogsDB;Integrated Security=True;Trust Server Certificate=True"
        });*/

        DomainLogsConnectionString =
        "workstation id=NorthWind_Moviles.mssql.somee.com;packet size=4096;user id=AZ_developer_SQLLogin_2;pwd=lip1fgttra;data source=NorthWind_Moviles.mssql.somee.com;persist security info=False;initial catalog=NorthWind_Moviles;TrustServerCertificate=True"
        });
        return new NorthWindDomainLogsContext(DbOptions);
    }
}