using Microsoft.EntityFrameworkCore;

namespace Sharara.Services.Kumusha
{
    class DatabaseContextOverride : Generated.DatabaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            string hostname = "true".Equals(System.Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")) ?
                "host.docker.internal" :
                "localhost";

            //  docker run --name some-mysql -p 20000:3306 -e MYSQL_ROOT_PASSWORD=Stupid-Password-123 -d mysql:latest
            string connString = $"server={hostname};port=20000;uid=root;pwd=Stupid-Password-123;database=test";

            optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));

        }
    }

}
