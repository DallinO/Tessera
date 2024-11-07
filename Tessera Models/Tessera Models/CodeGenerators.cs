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

        public static string StringOfDigits(int digits)
        {
            Random _rng = new();
            string id = string.Empty;

            for (int i = 0; i < digits; i++)
            {
                id += _rng.Next(9).ToString();
            }

            return id;
        }

        //public static string PurchaseOrder()
        //{
        //    Random _rng = new();
        //    string id = "PO-" + (DateTime.Now.Year % 100);

        //    int year = DateTime.Now.Year % 100;

        public static string GenerateNineDigitId()
        {
            string number;
            do
            {
                number = GenerateNumber();
            } while (!IsValid(number));

            return number;
        }

        private static string GenerateNumber()
        {
            char[] digits = "123456789".ToCharArray(); // Start with digits 1-9
            char[] result = new char[9];
            result[0] = digits[_random.Next(digits.Length)]; // Ensure it doesn't start with 0

            for (int i = 1; i < 9; i++)
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
            return count >= 3;
        }

        private static bool IsValid(string number)
        {
            // Check for at least 4 different digits
            if (number.Distinct().Count() < 4)
                return false;

            // Check that no digit repeats more than 3 times consecutively
            for (int i = 0; i < number.Length - 3; i++)
            {
                if (number[i] == number[i + 1] && number[i] == number[i + 2] && number[i] == number[i + 3])
                    return false;
            }

            return true;
        }




        //}
    }

}
