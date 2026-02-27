using System;
using System.IO;
using System.Security;
using DSInternals.Common.Properties;

namespace DSInternals.Common
{
    public static class Validator
    {
        public static void AssertEquals(string expectedValue, string actualValue, string paramName)
        {
            if(!String.Equals(expectedValue, actualValue, StringComparison.InvariantCulture))
            {
                string message = String.Format(Resources.UnexpectedValueMessage, actualValue, expectedValue);
                throw new ArgumentException(message, paramName);
            }
        }

        public static void AssertEquals(char expectedValue, char actualValue, string paramName)
        {
            if (expectedValue.CompareTo(actualValue) != 0)
            {
                string message = String.Format(Resources.UnexpectedValueMessage, actualValue, expectedValue);
                throw new ArgumentException(message, paramName);
            }
        }

        public static void AssertNotNull(object value, string paramName)
        {
            if(value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void AssertNotNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void AssertNotNullOrWhiteSpace(string value, string paramName)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void AssertLength(string value, int length, string paramName)
        {
            AssertNotNull(value, paramName);
            if(value.Length != length)
            {
                throw new ArgumentOutOfRangeException(paramName, value.Length, Resources.UnexpectedLengthMessage);
            }
        }

        public static void AssertMaxLength(SecureString password, int maxLength, string paramName)
        {
            AssertNotNull(password, paramName);
            if (password.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException(paramName, password.Length, Resources.InputLongerThanMaxMessage);
            }
        }

        public static void AssertMaxLength(string input, int maxLength, string paramName)
        {
            AssertNotNull(input, paramName);
            if (input.Length > maxLength)
            {
                throw new ArgumentOutOfRangeException(paramName, input.Length, Resources.InputLongerThanMaxMessage);
            }
        }

        public static void AssertMinLength(byte[] data, int minLength, string paramName)
        {
            AssertNotNull(data, paramName);
            if (data.Length < minLength)
            {
                var exception = new ArgumentOutOfRangeException(paramName, data.Length, Resources.InputShorterThanMinMessage);
                // DEBUG: exception.Data.Add("BinaryBlob", data.ToHex());
                throw exception;
            }
        }

        public static void AssertLength(byte[] value, long length, string paramName)
        {
            AssertNotNull(value, paramName);
            if (value.Length != length)
            {
                throw new ArgumentOutOfRangeException(paramName, value.Length, Resources.UnexpectedLengthMessage);
            }
        }

        public static void AssertFileExists(string filePath)
        {
            bool exists = File.Exists(filePath);
            if(!exists)
            {
                throw new FileNotFoundException(Resources.PathNotFoundMessage, filePath);
            }
        }

        public static void AssertDirectoryExists(string directoryPath)
        {
            bool exists = Directory.Exists(directoryPath);
            if (!exists)
            {
                throw new DirectoryNotFoundException(Resources.PathNotFoundMessage);
            }
        }
    }
}
