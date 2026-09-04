using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public Models.Laborer? Laborer { get; }

        public DuplicateEmailException()
        {
        }

        public DuplicateEmailException(string message)
            : base(message)
        {
        }

        public DuplicateEmailException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public DuplicateEmailException(string message, Models.Laborer model)
            : base(message)
        {
            this.Laborer = model;
        }
    }
}
