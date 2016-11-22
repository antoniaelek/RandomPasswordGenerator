using System;

namespace RandomPasswordGenerator.Models
{
    public class PasswordMin
    {
        public int Id { get; set; }

        public string PasswordText { get; set; }

        public string Hint { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public PasswordMin(Password pass)
        {
            Id = pass.Id;
            PasswordText = pass.PasswordText;
            Hint = pass.Hint;
            DateCreated = pass.DateCreated;
        }

    }
}