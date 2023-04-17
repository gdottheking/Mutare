using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Docs.Samples;

namespace Sharara.Services.Kumusha.Generated
{
    [Route("/api/v1/[controller]")]
    [Route("/api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly DatabaseContext databaseContext;

        public AddressController(IRepository repository, DatabaseContext databaseContext)
        {
            this.repository = repository;
            this.databaseContext = databaseContext;
        }

        [Route("count")]
        [HttpGet]
        public async Task<IActionResult> GetCountAsync()
        {
            return Ok(await repository.GetStudentCountAsync());
        }

        [Route("all")]
        [HttpGet]
        public async Task<IActionResult> GetAllAddressesAsync([FromQuery] int page = 0, [FromQuery] int count = 100)
        {
            return Ok(await repository.GetAllAddressesAsync(page, count));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var db = new DatabaseContextOverride();
            db.Database.EnsureCreated();

            // Console.WriteLine("Inserting address");
            var repo = new Generated.Repository(db);
            var ent = new Generated.AddressEntity
            {
                Line1 = new Random(DateTime.Now.Second).Next(100) + " Fairview Road",
                PostalCode = "GU12 6AS"
            };
            await repo.PutAddressAsync(ent);
            // Console.WriteLine($"Address added with id = {ent.Id}");

            var numAddr = await repo.GetAddressCountAsync();
            //Console.WriteLine($"There are {numAddr} addresses in the database");
            var addresses = await repo.GetAllAddressesAsync(0, 1000);
            foreach (var addr in addresses)
            {
                // Console.WriteLine(addr.Line1 + " " + addr.PostalCode);
            }

            return ControllerContext.MyDisplayRouteInfo();
        }

    }
}