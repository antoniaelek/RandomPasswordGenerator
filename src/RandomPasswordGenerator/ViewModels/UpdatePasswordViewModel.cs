using System.ComponentModel.DataAnnotations;

namespace RandomPasswordGenerator.Models
{
    public class UpdatePasswordViewModel
    {
        [Required]
        public string PasswordText { get; set; }

        public string Hint { get; set; }

    }
}