using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HopperEngine;

namespace HopperEngineTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void InitialState()
        {
            Pond p = new Pond();

            // Pond should contain 13 pads
            Assert.IsTrue(p.GetTotalPads() == 13);

            //// Pond should be empty right now
            Assert.IsTrue(p.GetPadState('A') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('B') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('C') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('D') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('E') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('F') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('G') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('H') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('I') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('J') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('K') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('L') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('M') == PadState.Empty);

            Assert.IsTrue(p.ValidRoute('A', 'B') == false);
            Assert.IsTrue(p.ValidRoute('A', 'C') == true);
            Assert.IsTrue(p.ValidRoute('K', 'I') == false);
            Assert.IsTrue(p.ValidRoute('M', 'G') == true);

            int red, green;
            Assert.IsTrue(p.GetFrogCounts(out red, out green) == 0);
        }

        [TestMethod]
        public void FrogHopTest()
        {
            Pond p = new Pond();

            p.SetPad('A', PadState.Red);
            Assert.IsTrue(p.GetPadState('A') == PadState.Red);

            p.SetPad('B', PadState.Green);
            Assert.IsTrue(p.GetPadState('B') == PadState.Green);

            int red, green;
            Assert.IsTrue(p.GetFrogCounts(out red, out green) == 2);
            Assert.IsTrue(red == 1 && green == 1);
            
            Assert.IsTrue(p.ValidHop('A', 'B') == false);
            Assert.IsTrue(p.ValidHop('A', 'C') == true);
            Assert.IsTrue(p.ValidHop('C', 'A') == false);
            Assert.IsTrue(p.ValidHop('B', 'A') == false);

            Assert.IsTrue(p.IsMovementPossible());

            List<char> pads = p.GetPossibleHops('A');
            Assert.IsTrue(pads.Count == 1 && pads.Contains('C'));
            
            p.Hop(new Hop('A', 'C'));
            Assert.IsTrue(p.GetPadState('A') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('B') == PadState.Empty);
            Assert.IsTrue(p.GetPadState('C') == PadState.Red);

            Assert.IsTrue(p.GetFrogCounts(out red, out green) == 1);
            Assert.IsTrue(red == 1);

            Assert.IsTrue(p.ValidHop('C', 'A') == false);
            Assert.IsTrue(p.ValidHop('C', 'M') == false);

            Assert.IsFalse(p.IsMovementPossible());
        }

        [TestMethod]
        public void GetPadWithInvalidName()
        {
            Pond p = new Pond();
            
            // Test GetPad with a non-valid pad name - should throw
            Assert.IsTrue(p.GetPadState('X') == PadState.Empty);
        }

        [TestMethod]
        public void HopStackTest()
        {
            HopStack hs = new HopStack();
            Assert.IsFalse(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());

            hs.Push(new Hop('A', 'C'));
            Assert.IsTrue(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());
            
            hs.Push(new Hop('C', 'M'));
            Assert.IsTrue(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());
            
            Hop h = hs.Undo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'C' && h.EndPad == 'M');
            Assert.IsTrue(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());
            
            h = hs.Undo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'A' && h.EndPad == 'C');
            Assert.IsFalse(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());

            h = hs.Redo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'A' && h.EndPad == 'C');
            Assert.IsTrue(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());

            h = hs.Redo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'C' && h.EndPad == 'M');
            Assert.IsTrue(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());

            h = hs.Undo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'C' && h.EndPad == 'M');
            Assert.IsTrue(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());

            hs.Push(new Hop('C', 'G'));
            Assert.IsTrue(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());

            h = hs.Undo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'C' && h.EndPad == 'G');
            Assert.IsTrue(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());

            h = hs.Undo();
            Assert.IsNotNull(h);
            Assert.IsTrue(h.StartPad == 'A' && h.EndPad == 'C');
            Assert.IsFalse(hs.HasUndo());
            Assert.IsTrue(hs.HasRedo());

            hs.Push(new Hop('A', 'K'));
            Assert.IsTrue(hs.HasUndo());
            Assert.IsFalse(hs.HasRedo());

        }

        [TestMethod]
        public void TaskTest()
        {
            Task t = new Task("1 : B : I A D G : I-E, A-G, E-I");
            Assert.IsNotNull(t);
            Assert.IsTrue(t.Number == 0);
            Assert.IsTrue(t.Difficulty == Task.Level.Beginner);
            Assert.IsTrue(t.Name == "1");
            Assert.IsTrue(t.Red == 'I');
            Assert.IsTrue(t.Greens[0] == 'A' && t.Greens[1] == 'D' && t.Greens[2] == 'G');
            Assert.IsTrue(t.Solution.Count == 3);
            Assert.IsTrue(t.Solution.ElementAt(0).StartPad == 'I' && t.Solution.ElementAt(0).EndPad == 'E');
            Assert.IsTrue(t.Solution.ElementAt(1).StartPad == 'A' && t.Solution.ElementAt(1).EndPad == 'G');
            Assert.IsTrue(t.Solution.ElementAt(2).StartPad == 'E' && t.Solution.ElementAt(2).EndPad == 'I');
        }
    }
}
