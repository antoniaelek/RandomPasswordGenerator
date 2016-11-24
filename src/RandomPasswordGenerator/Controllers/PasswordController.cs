using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using RandomPasswordGenerator.Models;

namespace RandomPasswordGenerator 
{
    [Route("api/[controller]/")]
    public class PasswordController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private string _lowerChar = "abcdefghijklmnopqrstuvwxyz";
        private string _upperChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string _digit = "1234567890";
        private string _nonAlphaNum = "!@#$%^&*()_-+=[{]};:<>|./?";
        private IConfigurationRoot _configuration;

        public PasswordController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
        }

        // POST: api/Password
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Post([FromBody]PasswordViewModel viewModel)
        {
            Logger.Fatal(this.Request.Log());
            if (ModelState.IsValid)
            {
                var plain = new string(GetRandomPass(viewModel));
                var encrypted = plain.Encrypt(_configuration.GetConnectionString("Enc"));
                var password = new Password()
                {
                    PasswordText = encrypted,
                    DateCreated = DateTime.Now,
                    Hint = viewModel.Hint
                };

                var user = await _userManager.GetUser(User.Identity.Name);
                if (user == null)
                    return new JsonResult(new
                    {
                        Success = true,
                        Password = plain,
                        DateCreated = password.DateCreated
                    });

                await SavePassword(password, user);

                return new JsonResult(new
                {
                    Success = true,
                    Id = password.Id,
                    Password = plain,
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
            Logger.Fatal(this.Request.Log());
            var isAuthorized = await CheckUserAuthorized(id); 
            if (isAuthorized.StatusCode != 200) return isAuthorized;
            
            // All was well
            var pass = await _context.Password.FirstOrDefaultAsync(x => x.Id == id);
            return new JsonResult(new
            {
                Success = true,
                Id = pass.Id,
                Password = pass.PasswordText.Decrypt(_configuration.GetConnectionString("Enc")),
                Hint = pass.Hint,
                UserId = pass.UserId,
                DateCreated = pass.DateCreated
            });
        }

        // PUT: api/Password/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<JsonResult> Put(int id, [FromBody]Password password)
        {
            Logger.Fatal(this.Request.Log());
            var isAuthorized = await CheckUserAuthorized(id);
            if (isAuthorized.StatusCode != 200) return isAuthorized;

            // All was well
            var pass = await _context.Password.FirstOrDefaultAsync(x => x.Id == id);
            
            // Update old pass
            pass.PasswordText = password.PasswordText.Encrypt(_configuration.GetConnectionString("Enc"));
            pass.Hint = password.Hint;

            // Save changes
            _context.Password.Update(pass);
            _context.SaveChanges();
            return 202.SuccessStatusCode();
        }

        // DELETE: api/Password/{id}
        [HttpDelete("{id}")]
        public async Task<JsonResult> Delete(int id)
        {
            Logger.Fatal(this.Request.Log());
            var isAuthorized = await CheckUserAuthorized(id);
            if (isAuthorized.StatusCode != 200) return isAuthorized;

            // All was well
            var pass = await _context.Password.FirstOrDefaultAsync(x => x.Id == id);
            _context.Password.Remove(pass);
            _context.SaveChanges();
            return 202.SuccessStatusCode();
        }

        private async Task<JsonResult> CheckUserAuthorized(int id)
        {
            // User null, how did we even get past the Authorize attribute?
            var user = await _userManager.GetUser(User.Identity.Name);
            if (user == null) return 401.ErrorStatusCode();
                
            var pass = await _context.Password.FirstOrDefaultAsync(x => x.Id == id);
            
            // Password with the given id doesn't exist
            if (pass == null) return 404.ErrorStatusCode();

            // This user has no right to fetch this password
            if (pass.UserId != user.Id) return 401.ErrorStatusCode();
            return 200.SuccessStatusCode();
        }

        private async Task SavePassword(Password password, ApplicationUser user)
        {
            password.User = user;
            password.UserId = user.Id;

            _context.Password.Add(password);
            
            await _context.SaveChangesAsync();
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