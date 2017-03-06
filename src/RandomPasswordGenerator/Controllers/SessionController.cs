using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;
using NLog;

namespace RandomPasswordGenerator
{
    [Route("api/[controller]/")]
    public class SessionController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private IConfigurationRoot _configuration;

        public SessionController(ApplicationDbContext context,
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


        // DELETE api/logout
        [HttpDelete]
        [Authorize]
        [RouteAttribute("api/logout")]
        public async Task Logout()
        {
            Logger.Fatal(this.Request.Log());
            await _signInManager.SignOutAsync();
        }


        // POST: api/login
        [HttpPost]
        [AllowAnonymous]
        [RouteAttribute("api/login")]
        public async Task<JsonResult> Login([FromBody]LoginViewModel viewModel)
        {
            Logger.Fatal(this.Request.Log());
            var key = this.Request.Headers.Keys.FirstOrDefault(h => h.ToLower() == "user-agent");
            var browser =  "";
            if (key != null) browser = this.Request.Headers[key];
            Logger.Info(this.Request.Path + " " + browser);
            if (viewModel.UserName == null && viewModel.Email == null)
            {
                ModelState.AddModelError("Email","Email or Username field is requried.");
                ModelState.AddModelError("Username","Email or Username field is requried.");
            }
            if (ModelState.IsValid)
            {
                ApplicationUser user = null;
                if (viewModel.Email != null) 
                {
                    user = await _userManager.FindByEmailAsync(viewModel.Email);
                    if (user == null) return 404.ErrorStatusCode();
                }
                else if (viewModel.UserName != null || 
                         (user == null && viewModel.UserName != null)) 
                    user = await _userManager.FindByNameAsync(viewModel.UserName);

                var result = await _signInManager.
                    PasswordSignInAsync(user.UserName,
                                        viewModel.Password,
                                        true, false);
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