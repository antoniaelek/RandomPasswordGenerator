using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomPasswordGenerator.Models
{
    public class Password
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PasswordText { get; set; }

        public string Hint { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public Password()
        {

        }
    }
}