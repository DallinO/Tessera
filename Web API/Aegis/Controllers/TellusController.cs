using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tellus.Models.Api;
using Tellus.Models.Business;
using Tellus.Models.Structures;

namespace Aegis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TellusController : ControllerBase
    {
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly DbContextFactory _dbFactory;
        private readonly ILogger<TellusController> _logger;

        public TellusController(UserManager<Scribe> userManager, SignInManager<Scribe> signInManager, TesseraDbContext dbContext, DbContextFactory dbFactory, ILogger<TellusController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _dbFactory = dbFactory;
            _logger = logger;
        }

        //[Authorize]
        [HttpGet("GetCustomerList")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiCustomerList))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerList([FromBody] ApiCustomerListRquest request)
        {
            _logger.LogInformation("FETCH CUSTOMERS CALLED");
            using (var dbContext = _dbFactory.CreateDbContext("tessera-pm-01"))
            {
                // Fetching all customer IDs from the Customers table
                var customerIds = await dbContext.Customers
                                         .OrderBy(c => c.Id) // Ensure consistent ordering
                                         .Skip((request.PageNumber - 1) * request.PageSize) // Skip the previous pages
                                         .Take(request.PageSize) // Take the specified number of records
                                         .Select(c => c.Id)
                                         .ToListAsync();

                // Return the list of customer IDs as JSON
                _logger.LogInformation("CUSTOMERS RETRIEVED");
                return Ok(new ApiCustomerList
                {
                    Customers = customerIds
                });
            }
        }

        //[Authorize]
        [HttpGet("GetCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiCustomer))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomer([FromBody] int customerId)
        {
            _logger.LogInformation("FETCH CUSTOMER CALLED");
            using (var dbContext = _dbFactory.CreateDbContext("tessera-pm-01"))
            {
                // Fetch the customer from the Customers table using the provided customerId
                var customer = await dbContext.Customers
                    .Where(c => c.Id == customerId)
                    .Select(c => new CustomerEntity
                    {
                        Id = c.Id,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        WorkNumber = c.WorkNumber,
                        CellNumber = c.CellNumber,
                        HomeNumber = c.HomeNumber,
                        Details = c.Details,
                        HousingType = c.HousingType,
                        AddressId = c.AddressId
                    })
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    _logger.LogWarning("Customer not found for ID: {CustomerId}", customerId);
                    return NotFound(
                        new ApiCustomer
                        {
                            Success = false,
                            Errors = new List<string>()
                            {
                                "Customer not found"
                            }

                        }); // Return 404 if the customer doesn't exist

                }

                _logger.LogInformation("CUSTOMER RETRIEVED: {CustomerId}", customer.Id);
                return Ok(new ApiCustomer
                {
                    Customer = customer
                });
            }
        }
    }
}
