﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orcus.Exceptions
{
    class OutOfRangeException : OrcusException
    {

        public OutOfRangeException()
            : base("The value specified was out of range.") 
        { }

    }
}
