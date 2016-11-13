using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RandomPasswordGenerator.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;
        
        [Required]
        public string Name { get; set; }

        public ICollection<Password> Passwords { get; set; } = new HashSet<Password>();

        public ApplicationUser()
        {
            
        }
    }
}