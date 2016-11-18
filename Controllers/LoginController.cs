using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        
        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<LoginController>();
        }

        // POST: api/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Post([FromBody]LoginViewModel viewModel)
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