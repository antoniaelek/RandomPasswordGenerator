using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using RandomPasswordGenerator.Models;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class RegisterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;

        public RegisterController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<RegisterController>();
        }

        // POST: api/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Post([FromBody]RegisterViewModel viewModel)
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
            if (!ModelState.Keys.Any()) 
                ModelState.AddModelError("Email","There already exists an account with that email.");
            var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Success = false, Verbose = allErrors});
            ret.StatusCode = 400;
            return ret;
        }
    }
}