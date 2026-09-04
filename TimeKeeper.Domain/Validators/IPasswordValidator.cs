using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Validators
{
    public interface IPasswordValidator
    {
        public string ErrorMessage { get; }

        bool IsValid();
    }
}
