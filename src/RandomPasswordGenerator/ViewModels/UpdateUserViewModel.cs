using System.ComponentModel.DataAnnotations;

namespace RandomPasswordGenerator.ViewModels
{
    public class UpdateUserViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
