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
using NLog;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class UserController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
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
            Logger.Fatal(this.Request.Log());
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
            Logger.Fatal(this.Request.Log());
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

        // POST: api/User
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Create([FromBody]RegisterViewModel viewModel)
        {
            Logger.Fatal(this.Request.Log());
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

        // PUT: api/User/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<JsonResult> Update(string id,[FromBody]UpdateUserViewModel viewModel)
        {
            Logger.Fatal(this.Request.Log());
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            if (user == null) return 404.ErrorStatusCode();

            var isAuthorized = await CheckUserAuthorized(user.UserName);
            if (isAuthorized.StatusCode != 200) return 401.ErrorStatusCode();

            user.Name = viewModel.Name;
            _context.Users.Update(user);
            _context.SaveChanges();

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

        // DELETE api/User/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<JsonResult> Delete(string id)
        {
            Logger.Fatal(this.Request.Log());
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            if (user == null) return 404.ErrorStatusCode();

            var isAuthorized = await CheckUserAuthorized(user.UserName);
            if (isAuthorized.StatusCode != 200) return 401.ErrorStatusCode();

            await _userManager.DeleteAsync(user);
            return 200.SuccessStatusCode();
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