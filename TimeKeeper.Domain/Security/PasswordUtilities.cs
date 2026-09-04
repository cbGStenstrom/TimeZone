using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Validators;

namespace TimeKeeper.Domain.Security
{
    public static class PasswordUtilities
    {

        /// <summary>
        /// Applies password validation to the <paramref name="password"/> argument
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool PasswordIsValid(Laborer user, out string errorMessage)
        {
            bool isValid = true;
            errorMessage = string.Empty;

            if (user.Password != null)
            {
                IPasswordValidator[] validators =
                {
                    new PasswordConfirmValidator(user.Password, user.PasswordConfirm),
                    new PasswordNumberValidator(user.Password),
                    new PasswordSpecialCharacterValidator(user.Password)
                };

                isValid = PasswordPassesValidation(validators, out errorMessage);
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Applies the validators in the <paramref name="validators"/> argument and returns FALSE 
        /// if any of them fail.
        /// </summary>
        /// <param name="validators"></param>
        /// <returns></returns>
        public static bool PasswordPassesValidation(IPasswordValidator[] validators, out string errorMessage)
        {
            bool isValid = true;
            errorMessage = string.Empty;

            foreach (IPasswordValidator validator in validators)
            {
                isValid = validator.IsValid();
                if(!isValid)
                {
                    errorMessage = validator.ErrorMessage;
                    break;
                }
            }

            return isValid;
        }


        /// <summary>
        /// Returns a hashed version of the user' password to be stored in the database.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string username, string password)
        {
            PasswordHasher<string> hasher = new();
            string hashedPassword = hasher.HashPassword(username, password);
            return hashedPassword;
        }

        /// <summary>
        ///  Compares the stored/hashed password to another string to determine if they are the same
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="username"></param>
        /// <param name="passwordToValidate"></param>
        /// <returns></returns>
        public static bool PasswordsMatch(string hashedPassword, string username, string passwordToValidate)
        {
            PasswordHasher<string> hasher = new();
            PasswordVerificationResult result = hasher.VerifyHashedPassword(username, hashedPassword, passwordToValidate);
            return result == PasswordVerificationResult.Success;
        }

        /// <summary>
        ///  Evaluates the <paramref name="password"/> argument to determine if it contains any of 
        ///  the required sepecial characters.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="requiredCount">
        ///  The number of special characters that shouls appear in the <paramref name="password"/> 
        ///   argument.
        ///  </param>
        /// <returns></returns>
        static bool PasswordContainsSpecialCharacters(string password, int requiredCount = 1)
        {
            bool isValid = false;

            char[] specialCharacters = { '~', '@', '#', '$', '%', '^', '&', '*', '(', ')', '?', '<', '>', '{', '}', '[', ']' };
            int specialCharacterCount = 0;

            foreach (char c in specialCharacters)
            {
                bool containsChar = password.Contains(c);
                if (containsChar) { specialCharacterCount++; }

                if(specialCharacterCount == requiredCount)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

    }
}
