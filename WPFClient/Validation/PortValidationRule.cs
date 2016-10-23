﻿using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WPFClient.Validation
{
    public class PortValidationRule:ValidationRule
    {
        private const string ValidationPatternPort = "^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Valid port number required");

            return Regex.IsMatch(value.ToString(), ValidationPatternPort)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, "Valid port number required");
        }
    }
}