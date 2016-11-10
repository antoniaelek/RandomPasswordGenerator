using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator {
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private static bool _databaseChecked;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
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
                else
                {
                    AddErrors(result);
                    return new JsonResult(new { Success = false
                        , Name = viewModel.Name, Email=viewModel.Email, Password = viewModel.Password
                     });
                }
            }

            return new JsonResult(new { Success = false
            , Name = viewModel.Name, Email=viewModel.Email, Password = viewModel.Password });
        }

        // POST: api/Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Login(LoginViewModel viewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                //var isPersistent = viewModel.IsPersistent ?? false;
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
                        Name = user.Name,
                        Id = user.Id,
                        Email = user.Email,
                        DateCreated = user.DateCreated,
                        Roles = tmp
                    });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid e-mail or password.");
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
                var user = await _userManager
                    .FindByNameAsync(User.Identity.Name);
                return new JsonResult(
                    new {
                            UserId = user.Id,
                            Roles = await _userManager.GetRolesAsync(user)
                        }
                );
            }
            
            var tmp = new JsonResult(new { Success = false });
            tmp.StatusCode = 401;
            return tmp;
        }

        // GET api/Account/Logout
        [HttpGet]
        [Authorize]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #endregion
    }
}