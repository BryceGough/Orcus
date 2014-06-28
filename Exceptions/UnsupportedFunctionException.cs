using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orcus.Exceptions
{
    public class UnsupportedFunctionException : OrcusException
    {

        public UnsupportedFunctionException()
            : base("The device driver the system is using does not support this function.")
        { }

    }
}
