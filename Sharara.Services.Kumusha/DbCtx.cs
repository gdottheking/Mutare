using Microsoft.EntityFrameworkCore;

namespace Sharara.Services.Kumusha
{
    class DbCtx : Generated.DatabaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
             //  docker run --name some-mysql -p 20000:3306 -e MYSQL_ROOT_PASSWORD=Stupid-Password-123 -d mysql:latest
            string connString = "server=localhost;port=20000;uid=root;pwd=Stupid-Password-123;database=test";
            if (!string.IsNullOrWhiteSpace(connString))
            {
                optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));
            }
        }
    }

}
