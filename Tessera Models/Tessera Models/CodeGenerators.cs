using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.CodeGenerators
{
    public static class CodeGen
    {
        private static Random _random = new Random();

        public static string GenerateNumberOfLength(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than zero.", nameof(length));

            string number;
            do
            {
                number = GenerateNumber(length);
            } while (!IsValid(number));

            return number;
        }

        private static string GenerateNumber(int length)
        {
            char[] digits = "123456789".ToCharArray(); // Start with digits 1-9 for the first digit
            char[] result = new char[length];
            result[0] = digits[_random.Next(digits.Length)]; // Ensure it doesn't start with 0

            for (int i = 1; i < length; i++)
            {
                char nextDigit;
                do
                {
                    nextDigit = (char)_random.Next('0', '9' + 1);
                }
                while (IsSameDigitTooLong(result, i, nextDigit));
                result[i] = nextDigit;
            }

            return new string(result);
        }

        private static bool IsSameDigitTooLong(char[] number, int index, char digit)
        {
            int count = 0;
            for (int i = index - 1; i >= 0 && number[i] == digit; i--)
            {
                count++;
            }
            return count >= 3; // Adjust this if you want to allow more or fewer consecutive repeats
        }

        private static bool IsValid(string number)
        {
            // Check for at least 4 different digits (or less for shorter numbers)
            int requiredDistinctDigits = Math.Min(4, number.Length);
            if (number.Distinct().Count() < requiredDistinctDigits)
                return false;

            // Check that no digit repeats more than 3 times consecutively
            for (int i = 0; i < number.Length - 3; i++)
            {
                if (number[i] == number[i + 1] 
                    && number[i] == number[i + 2] 
                    && number[i] == number[i + 3])
                    return false;
            }

            return true;
        }

    }

}
