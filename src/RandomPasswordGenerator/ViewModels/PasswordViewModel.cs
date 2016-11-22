using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RandomPasswordGenerator
{
    public class PasswordViewModel : IValidatableObject
    {
        public int Length { get; set; }
        public bool LowerChars { get; set; }
        public bool UpperChars { get; set; }
        public bool Digits { get; set; }
        public bool Symbols { get; set; }
        public string Hint { get; set; }

        public PasswordViewModel()
        {
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            var min = 0;
            min += LowerChars ? 1 : 0;
            min += UpperChars ? 1 : 0;
            min += Digits ? 1 : 0;
            min += Symbols ? 1 : 0;
            if (min > Length)
            {
                yield return new ValidationResult(
                    min + " different characters requested, but length specified as" + Length,
                    new[] { "Length" });
            }
        }
    }
}