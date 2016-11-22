using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RandomPasswordGenerator.Models;

namespace RandomPasswordGenerator
{
    public static class Utils
    {
        public static Dictionary<string, string> ValidationErrors(this ModelStateDictionary ModelState)
        {
            var keys = ModelState.Keys;
            var allErrors = new Dictionary<string, string>();

            foreach(var key in keys)
            {
                ModelStateEntry val = null;
                if (!ModelState.TryGetValue(key, out val)) continue;
                allErrors.Add(key,val.Errors.Select(err => err.ErrorMessage).FirstOrDefault());
            }
            return allErrors;
        }

        public static JsonResult ErrorStatusCode(this int status)
        {
            var ret = new JsonResult(new
            {
                Success = false
            });
            ret.StatusCode = status;
            return ret;
        }

        public static JsonResult SuccessStatusCode(this int status)
        {
            var ret = new JsonResult(new
            {
                Success = true
            });
            ret.StatusCode = status;
            return ret;
        }

        public static async Task<ApplicationUser> GetUser(this UserManager<ApplicationUser> _userManager, string name)
        {
            //var name = User.Identity.Name;
            if (name == null) return null;
            var user = await _userManager.FindByNameAsync(name);
            if (user == null) return null;
            return user;
        }

        public static string Encrypt(this string clearText, string key)
        {
            //var EncryptionKey = _configuration.GetConnectionString("Enc");
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(this string cipherText, string key)
        {
            //var EncryptionKey = _configuration.GetConnectionString("Enc");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}