using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orcus.Exceptions
{
    public class OrcusException : Exception
    {

        public OrcusException(string msg)
            : base ("Orcus has encountered an error: " + msg)
        { }

    }
}
