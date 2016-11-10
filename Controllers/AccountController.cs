using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator {
    [Route("api/[controller]/[action]/")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        // POST: api/Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Register([Bind]RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = viewModel.Email,
                    Email = viewModel.Email,
                    Name = viewModel.Name
                };

                var result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    return new JsonResult(new {
                        Success = true,
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        DateCreated = user.DateCreated,
                    });
                }
            }
            return new JsonResult(new { Success = false });
        }

        // POST: api/Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Login(LoginViewModel viewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.
                    PasswordSignInAsync(viewModel.Email,
                                        viewModel.Password,
                                        viewModel.IsPersistent, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(viewModel.Email);
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
            return new JsonResult(new { Success = false });
        }

        // GET api/Account/Session
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> Session()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                return new JsonResult(new { Id = user.Id });
            }
            return new JsonResult(new { Success = false });
        }

        // GET api/Account/Logout
        [HttpGet]
        [Authorize]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}