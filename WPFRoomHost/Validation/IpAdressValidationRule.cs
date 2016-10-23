using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WPFRoomHost.Validation
{
    public class IpAdressValidationRule:ValidationRule
    {
        private const string ValidationPatternIpAdress = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value==null) return new ValidationResult(false, "Valid IP address required");
            if(string.Equals("localhost",value.ToString())) return ValidationResult.ValidResult;

            return Regex.IsMatch(value.ToString(), ValidationPatternIpAdress)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Valid IP address required");
        }
    }
}