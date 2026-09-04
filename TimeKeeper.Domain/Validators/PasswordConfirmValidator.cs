using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Validators
{
    public class PasswordConfirmValidator : IPasswordValidator
    {
        #region properties

        public string ErrorMessage { get; private set; }

        public string Password1 { get; }

        public string Password2 { get; }

        #endregion properties

        #region data
        #endregion data

        #region ctor

        public PasswordConfirmValidator(string password1, string password2)
        {
            Password1 = password1;
            Password2 = password2;
        }

        #endregion ctor

        #region private
        #endregion private

        #region public

        public bool IsValid()
        {
            if (Password1 == null || Password2 == null)
            {
                this.ErrorMessage = "Must confirm Password.";
                return false;
            }

            if (Password1 != Password2)
            {
                this.ErrorMessage = "Passwords did not match";
                return false;
            }
            else
            {
                return true;
            }   
        }

        #endregion public
    }
}
