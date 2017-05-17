using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ModernAudioTagger.Validation
{
    class NumberValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            // Here you make your validation using the value object.
            // If you want to check if the object is only numbers you can
            // Use some built-in method
            string blah = value != null ? value.ToString() : String.Empty;
            int num;
            bool isNum = int.TryParse(blah, out num);

            if (isNum) return new ValidationResult(true, null);
            else return new ValidationResult(false, "It's not a number");
        }
    }
}
