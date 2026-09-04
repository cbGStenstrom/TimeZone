using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Validators
{
    public class PasswordNumberValidator : IPasswordValidator
    {
        public string ErrorMessage { get { return $"Password does not contain at least {NumberCount} number(s)."; } }

        public int NumberCount { get; }

        public string Password { get; }

        public PasswordNumberValidator(string password, int numberCount = 1)
        {
            Password = password;
            NumberCount = numberCount;
        }

        public bool IsValid()
        {

            bool isValid = false;
            int[] ints = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            int numberCount = 0;

            foreach (int num in ints)
            {
                bool containsNum = this.Password.Contains(num.ToString());
                if (containsNum) { numberCount++; }

                if (numberCount == this.NumberCount)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }
    }
}
