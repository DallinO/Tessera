using Microsoft.AspNetCore.Http;
using System.Collections;
using Tessera.Models.Authentication;

namespace Aegis.UnitTests
{
    public class RegisterTestCases : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            /*********************
             * First Name Missing
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /***********************
             * First Name Too Short
             ***********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "J", // Too short (1 character)
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /**********************
             * First Name Too Long
             **********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = new string('A', 51), // Too long (51 characters)
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*****************************************
             * First Name Invalid Character - Symbol
             *****************************************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "J@ne", // Invalid character (@)
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*****************************************
             * First Name Invalid Character - Number
             *****************************************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "J1ne", // Invalid character (1)
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Last Name Missing
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = " ",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Last Name Too Short
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "John",
                    LastName = "D", // Too short (1 character)
                    Email = "john.doe@example.com",
                    ConfirmEmail = "john.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Last Name Too Long
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "John",
                    LastName = new string('B', 51), // Too long (51 characters)
                    Email = "john.doe@example.com",
                    ConfirmEmail = "john.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /***************************************
             * Last Name Contains Invalid Character
             ***************************************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "John",
                    LastName = "Doe#123", // Invalid character '#'
                    Email = "john.doe@example.com",
                    ConfirmEmail = "john.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Last Name Contains Invalid Character
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "John",
                    LastName = "Doe2", // Invalid character '2'
                    Email = "john.doe@example.com",
                    ConfirmEmail = "john.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Email Missing
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = " ",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Confirm Email Missing
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = " ",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Email Too Long
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = new string('a', 101) + "@example.com", // Too long (101 characters)
                    ConfirmEmail = new string('a', 101) + "@example.com", // Too long (101 characters)
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };


            /*********************
             * Invalid Email Format
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@com", // Invalid format
                    ConfirmEmail = "jane.doe@com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Email and Confirm Email Mismatch
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "different.email@example.com", // Mismatch
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status400BadRequest
            };


            /*********************
             * Password Missing
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = " ",         // No password
                    ConfirmPassword = " "   
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Password Length Too Short
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Short1", // Too short (less than 8 characters)
                    ConfirmPassword = "Short1"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Password Missing Special Character
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123", // Missing special character
                    ConfirmPassword = "Password123"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Password Missing Uppercase Letter
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "password123!", // Missing uppercase letter
                    ConfirmPassword = "password123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Password Missing Digit
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password!", // Missing digit
                    ConfirmPassword = "Password!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Confirm Password Mismatch
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "DifferentPassword123!"
                },
                StatusCodes.Status400BadRequest
            };

            /*********************
             * Confirm Password Empty
             *********************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = " "
                },
                StatusCodes.Status400BadRequest
            };

            /****************************
             * Valid Register Request OK
             ****************************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    ConfirmEmail = "alice.smith@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status200OK
            };

            /**********************************
             * Valid Register Request Conflict
             **********************************/
            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    ConfirmEmail = "alice.smith@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status409Conflict
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }





    public class DeleteTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                "c54739ff-efc1-4956-ba33-cdc79d9e81f8",
                StatusCodes.Status200OK
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
