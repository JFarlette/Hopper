using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HopperEngine
{
    public enum PadState
    {
        Empty,
        Red,
        Green
    }

    public enum PadSelection
    {
        None,
        Option,
        Selected
    }

    public class Pad
    {
        public Pad(char id, PadState state, PadSelection selection)
        {
            ID = id;
            State = state;
            Selection = selection;
        }

        public char ID { get; set; }
        public PadState State { get; set; }
        public PadSelection Selection { get; set; }
    }
}
