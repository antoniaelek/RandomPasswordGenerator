using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RandomPasswordGenerator.Models;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IConfigurationRoot _configuration;

        public UserController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
        }

        // POST: api/User/{id}
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> Get(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            return new JsonResult(new
            {
                Success = true,
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                DateCreated = user.DateCreated
            });
        }
    }
}