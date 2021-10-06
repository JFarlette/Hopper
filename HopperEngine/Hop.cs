using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HopperEngine
{
    public class Hop  // A hop from one pad to another
    {
        public Hop(char startPad, char endPad)
        {
            StartPad = startPad;
            EndPad = endPad;
        }

        public char StartPad;
        public char EndPad;
    }
}

