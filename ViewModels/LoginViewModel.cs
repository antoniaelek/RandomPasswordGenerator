using System.ComponentModel.DataAnnotations;

namespace RandomPasswordGenerator.ViewModels
{
    public class LoginViewModel
    {
        
        [EmailAddress]
        public string Email { get; set; }
        
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsPersistent { get; set; }
    }
}
