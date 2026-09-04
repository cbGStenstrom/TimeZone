using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Exceptions
{
    internal class LaborerNotFoundException : Exception
    {
        public Models.Laborer? Laborer { get; }

        public LaborerNotFoundException()
        {
        }

        public LaborerNotFoundException(string message)
            : base(message)
        {
        }

        public LaborerNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public LaborerNotFoundException(string message, Models.Laborer model)
            : base(message)
        {
            this.Laborer = model;
        }
    }
}
