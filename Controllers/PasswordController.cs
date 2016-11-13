using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RandomPasswordGenerator.Models;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class PasswordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private string _lowerChar = "abcdefghijklmnopqrstuvwxyz";
        private string _upperChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string _digit = "1234567890";
        private string _nonAlphaNum = "!@#$%^&*()_-+=[{]};:<>|./?";

        public PasswordController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/Password
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Post([FromBody]PasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var password = new Password()
                {
                    PasswordText = new string(GetRandomPass(viewModel)),
                    DateCreated = DateTime.Now,
                    Hint = viewModel.Hint
                };

                var user = await GetCurrentUser();
                if (user == null)
                    return new JsonResult(new
                    {
                        Success = true,
                        Password = password.PasswordText,
                        DateCreated = password.DateCreated
                    });

                await SavePassword(password, user);

                return new JsonResult(new
                {
                    Success = true,
                    Id = password.Id,
                    Password = password.PasswordText,
                    Hint = password.Hint,
                    UserId = password.UserId,
                    DateCreated = password.DateCreated
                });
            }
            else
            {
                var allErrors = ModelState.ValidationErrors();
                var ret = new JsonResult(new { Success = false, Verbose = allErrors});
                ret.StatusCode = 400;
                return ret;
            }
        }

        // GET: api/Password/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<JsonResult> Get([FromUri]int id)
        {
            // User null, how did we even get past the Authorize attribute?
            var user = await GetCurrentUser();
            if (user == null) return 401.ErrorStatusCode();
                
            var pass = await _context.Password.FirstOrDefaultAsync(x => x.Id == id);
            
            // Password with the given id doesn't exist
            if (pass == null) return 404.ErrorStatusCode();

            // This user has no right to fetch this password
            if (pass.UserId != user.Id) return 401.ErrorStatusCode();

            // All was well
            return new JsonResult(new
            {
                Success = true,
                Id = pass.Id,
                Password = pass.PasswordText,
                Hint = pass.Hint,
                UserId = pass.UserId,
                DateCreated = pass.DateCreated
            });
        }

        private async Task SavePassword(Password password, ApplicationUser user)
        {
            password.User = user;
            password.UserId = user.Id;

            _context.Password.Add(password);
            
            await _context.SaveChangesAsync();
        }

        private async Task<ApplicationUser> GetCurrentUser()
        {
            var name = User.Identity.Name;
            if (name == null) return null;
            var user = await _userManager.FindByNameAsync(name);
            if (user == null) return null;
            return user;
        }

        private char[] GetRandomPass(PasswordViewModel viewModel)
        {
            char[] pass = new char[viewModel.Length];
            var valid = new StringBuilder();
            var startIndex = 0;

            // Satisfy all conditions on password
            if (viewModel.LowerChars) SatisfyCondition(_lowerChar,ref valid,ref startIndex,ref pass);
            if (viewModel.UpperChars) SatisfyCondition(_upperChar,ref valid,ref startIndex,ref pass);
            if (viewModel.Digits) SatisfyCondition(_digit,ref valid,ref startIndex,ref pass);
            if (viewModel.Symbols) SatisfyCondition(_nonAlphaNum,ref valid,ref startIndex,ref pass);

            // Construct the rest of the password with random valid characters
            for(int i = startIndex; i < viewModel.Length;i++)
            {
                pass[i] = valid[GetNextInt(valid.Length)];
            }

            // Shuffle
            return pass.OrderBy(x => GetNextInt(Int32.MaxValue)).ToArray();
        }

        private void SatisfyCondition (string chars, ref StringBuilder allValidChars, ref int startIndex, ref char[] pass) 
        {
            allValidChars.Append(chars);
            pass[startIndex] = chars.ToCharArray()[GetNextInt(chars.Length)];
            startIndex += 1;
        }
        
        private int GetNextInt(int max)
        {
            using (var gen = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                gen.GetBytes(data);
                return BitConverter.ToUInt16(data, 0) % max;
            }
        }
    }
}