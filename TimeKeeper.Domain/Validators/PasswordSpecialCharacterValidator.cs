using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Validators
{
    public class PasswordSpecialCharacterValidator : IPasswordValidator
    {
        #region properties

        public string ErrorMessage { get { return $"Password does not contain at least {CharacterCount} special character(s)."; } }

        public int CharacterCount { get; }

        public string Password { get; }

        #endregion properties

        #region data
        #endregion data

        #region ctor

        public PasswordSpecialCharacterValidator(string password, int characterCount = 1)
        {
            Password = password;
            CharacterCount = characterCount;
        }

        #endregion ctor

        #region private
        #endregion private

        #region public

        public bool IsValid()
        {
            bool isValid = false;

            char[] specialCharacters = { '~', '@', '#', '$', '%', '^', '&', '*', '(', ')', '?', '<', '>', '{', '}', '[', ']' };
            int specialCharacterCount = 0;

            foreach (char c in specialCharacters)
            {
                bool containsChar = this.Password.Contains(c);
                if (containsChar) { specialCharacterCount++; }

                if (specialCharacterCount == this.CharacterCount)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }

        #endregion public
    }
}
