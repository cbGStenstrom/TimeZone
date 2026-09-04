using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Exceptions
{
    public class PasswordValidationException : Exception
    {

        public Models.Laborer? Laborer { get; }

        public PasswordValidationException()
        {
        }

        public PasswordValidationException(string message)
            : base(message)
        {
        }

        public PasswordValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public PasswordValidationException(string message, Models.Laborer model)
            : base(message)
        {
            this.Laborer = model;
        }
    }
}
