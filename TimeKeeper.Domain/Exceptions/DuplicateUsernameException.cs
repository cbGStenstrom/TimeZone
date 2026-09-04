using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Exceptions
{
    public class DuplicateUsernameException :Exception
    {
        public Models.Laborer? Laborer { get; }

        public DuplicateUsernameException()
        {
        }

        public DuplicateUsernameException(string message)
            : base(message)
        {
        }

        public DuplicateUsernameException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public DuplicateUsernameException(string message, Models.Laborer model)
            : base(message)
        {
            this.Laborer = model;
        }
    }
}
