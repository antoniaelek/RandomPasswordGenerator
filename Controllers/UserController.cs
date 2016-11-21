using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private IConfigurationRoot _configuration;

        public UserController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<UserController>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
        }

        // GET: api/User/{id}
        [HttpGet("{id}")]
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

        // GET api/User
        [HttpGet]
        [Authorize]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        // POST: api/User
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Register([FromBody]RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = viewModel.UserName,
                    Email = viewModel.Email,
                    Name = viewModel.Name
                };

                var result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    return new JsonResult(new {
                        Success = true,
                        Id = user.Id,
                        UserName = user.UserName,
                        Name = user.Name,
                        Email = user.Email,
                        DateCreated = user.DateCreated,
                    });
                }
            
            }
            if (!ModelState.Keys.Any()) 
                ModelState.AddModelError("Email","There already exists an account with that email.");
            var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Success = false, Verbose = allErrors});
            ret.StatusCode = 400;
            return ret;
        }

        // PUT: api/User
        [HttpPut]
        [AllowAnonymous]
        public async Task<JsonResult> Login([FromBody]LoginViewModel viewModel)
        {
            if (viewModel.UserName == null && viewModel.Email == null)
            {
                ModelState.AddModelError("Email","Email or Username field is requried.");
                ModelState.AddModelError("Username","Email or Username field is requried.");
            }
            if (ModelState.IsValid)
            {
                ApplicationUser user = null;
                if (viewModel.Email != null) user = await _userManager.FindByEmailAsync(viewModel.Email);
                else if (viewModel.UserName != null || 
                         (user == null && viewModel.UserName != null)) 
                    user = await _userManager.FindByNameAsync(viewModel.UserName);

                var result = await _signInManager.
                    PasswordSignInAsync(user.UserName,
                                        viewModel.Password,
                                        viewModel.IsPersistent, false);
                if (result.Succeeded)
                {
                    user = await _userManager.FindByEmailAsync(user.Email);
                    var tmp = _userManager.GetRolesAsync(user);
                    return new JsonResult(new
                    {
                        Success = true,
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        DateCreated = user.DateCreated,
                    });
                }
            }
            if (!ModelState.Keys.Any()) 
            {
                ModelState.AddModelError("Password","Invalid email or password.");
                ModelState.AddModelError("Email","Invalid email or password.");
            }
            var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Success = false, Verbose = allErrors });
            ret.StatusCode = 400;
            return ret;
        }
    }
}