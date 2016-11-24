using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WPFClient.Validation
{
    public class IpAdressValidationRule:ValidationRule
    {
        private const string ValidationPatternIpAdress = @"((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?):(\d){4}";
        private const string ValidationPetternLocalhost = @"localhost:[0-9]{4}";
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value==null) return new ValidationResult(false, "Valid IP address required");
            

            return Regex.IsMatch(value.ToString(), ValidationPatternIpAdress) | Regex.IsMatch(value.ToString(),ValidationPetternLocalhost)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Valid IP address required");
        }
    }
}