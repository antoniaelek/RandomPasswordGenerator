using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    }
}