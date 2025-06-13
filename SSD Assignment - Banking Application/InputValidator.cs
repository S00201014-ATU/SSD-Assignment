using System;
using System.Text.RegularExpressions;

namespace Banking_Application
{
    public static class InputValidator
    {
        // Validate name - only letters, spaces, apostrophes, and hyphens
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow letters, spaces, apostrophes, and hyphens only
            return Regex.IsMatch(name.Trim(), @"^[a-zA-Z\s'\-]+$") && name.Trim().Length <= 100;
        }

        // Validate address line - basic alphanumeric with common punctuation
        public static bool IsValidAddress(string address, bool required = true)
        {
            if (string.IsNullOrWhiteSpace(address))
                return !required; // Optional fields can be empty

            // Allow letters, numbers, spaces, and common punctuation
            return Regex.IsMatch(address.Trim(), @"^[a-zA-Z0-9\s,.\-'#/]+$") && address.Trim().Length <= 200;
        }

        // Validate town/city name
        public static bool IsValidTown(string town)
        {
            if (string.IsNullOrWhiteSpace(town))
                return false;

            // Similar to name validation but allow numbers for cases like "District 1"
            return Regex.IsMatch(town.Trim(), @"^[a-zA-Z0-9\s'\-]+$") && town.Trim().Length <= 100;
        }

        // Validate monetary amount
        public static bool IsValidMonetaryAmount(double amount, bool allowZero = true)
        {
            if (!allowZero && amount <= 0)
                return false;

            if (allowZero && amount < 0)
                return false;

            // Check for reasonable maximum (prevent overflow/unrealistic amounts)
            return amount <= 999999999.99;
        }

        // Validate account number format (GUID)
        public static bool IsValidAccountNumber(string accountNo)
        {
            if (string.IsNullOrWhiteSpace(accountNo))
                return false;

            return Guid.TryParse(accountNo, out _);
        }

        // Sanitize string input (trim and basic cleanup)
        public static string SanitizeString(string input)
        {
            if (input == null)
                return string.Empty;

            return input.Trim();
        }

        // Validate percentage (for interest rates)
        public static bool IsValidPercentage(double percentage)
        {
            return percentage >= 0 && percentage <= 100;
        }
    }
}