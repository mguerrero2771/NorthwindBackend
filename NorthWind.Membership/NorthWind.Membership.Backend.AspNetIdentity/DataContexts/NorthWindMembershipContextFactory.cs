using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using NorthWind.Membership.Backend.AspNetIdentity.Options;

namespace NorthWind.Membership.Backend.AspNetIdentity.DataContexts
{
    internal class NorthWindMembershipContextFactory :
 IDesignTimeDbContextFactory<NorthWindMembershipContext>
    {
        public NorthWindMembershipContext CreateDbContext(string[] args)
        {
            IOptions<MembershipDBOptions> DbOptions =
            Microsoft.Extensions.Options.Options.Create(
            new MembershipDBOptions()
            {
                /*ConnectionString =
            "Data Source=SEBAS;Initial Catalog=NorthWindUsersDB;Integrated Security=True;Trust Server Certificate=True"
            });*/

            ConnectionString =
           "workstation id=NorthWind_Moviles.mssql.somee.com;packet size=4096;user id=AZ_developer_SQLLogin_2;pwd=lip1fgttra;data source=NorthWind_Moviles.mssql.somee.com;persist security info=False;initial catalog=NorthWind_Moviles;TrustServerCertificate=True"
            });

            return new NorthWindMembershipContext(DbOptions);
        }
    }
}
