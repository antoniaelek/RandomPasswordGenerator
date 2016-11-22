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
using System.Collections.Generic;

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

        // GET: api/User/{id}/passwords
        [HttpGet("{id}/passwords")]
        [Authorize]
        public async Task<JsonResult> GetPasswords(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            var isAuthorized = await CheckUserAuthorized(user.UserName);
            if (isAuthorized.StatusCode != 200) return isAuthorized;
            
            var passwords = _context.Password.Where(p => p.User.UserName == id);
            var minimisedPass = new List<PasswordMin>();
            foreach(var pass in passwords) 
            {
                pass.PasswordText = pass.PasswordText.Decrypt(_configuration.GetConnectionString("Enc"));
                minimisedPass.Add(new PasswordMin(pass));
            }

            return new JsonResult(new
            {
                Success = true,
                Passwords = minimisedPass
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

        private async Task<JsonResult> CheckUserAuthorized(string id)
        {
            // User null, how did we even get past the Authorize attribute?
            var user = await _userManager.GetUser(User.Identity.Name);
            if (user == null) return 401.ErrorStatusCode();

            // This user is not the one with the specified id
            if (user.UserName != id) return 401.ErrorStatusCode();
            return 200.SuccessStatusCode();
        }
    }
}