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
            var ret = new JsonResult(new { Success = false });
            ret.StatusCode = 400;
            return ret;
        }
    }
}