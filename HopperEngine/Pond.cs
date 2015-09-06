using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HopperEngine
{
    public class Pond
    {
        public class PadRoute
        {
            public PadRoute(char end1, char middle, char end2)
            {
                m_end1 = end1;
                m_middle = middle;
                m_end2 = end2;
            }

            public char End1 { get { return m_end1; } }
            public char Middle { get { return m_middle; } }
            public char End2 { get { return m_end2; } }

            private char m_end1;
            private char m_middle;
            private char m_end2;

            internal bool IsRoute(char start, char finish)
            {
                return (m_end1 == start && m_end2 == finish) 
                    || (m_end1 == finish && m_end2 == start);
            }

            internal char GetOppositePad(char pad)
            {
                if (pad == m_end1) return m_end2;
                else if (pad == m_end2) return m_end1;
                else
                {
                    throw new ArgumentException(String.Format("Pad {0} not an end of the route {1}", ToString()));
                }
            }

            public override string ToString()
            {
                return String.Format("{1}-{2}-{3}", m_end1, m_middle, m_end2);
            }

        }

        static SortedSet<char> sm_padNames 
            = new SortedSet<char>() 
                { 'A', 'B', 'C', 
                     'D', 'E', 
                  'F', 'G', 'H', 
                     'I', 'J', 
                  'K', 'L', 'M' };

        static Dictionary<char, byte> sm_padMap;

        static PadRoute[] sm_routes
            = new PadRoute[] 
            {
                // Horizontal
                new PadRoute('A', 'B', 'C'),
                new PadRoute('F', 'G', 'H'),
                new PadRoute('K', 'L', 'M'),

                // Vertical
                new PadRoute('A', 'F', 'K'),
                new PadRoute('B', 'G', 'L'),
                new PadRoute('C', 'H', 'M'),

                // Diamond
                new PadRoute('B', 'E', 'H'),
                new PadRoute('H', 'J', 'L'),
                new PadRoute('L', 'I', 'F'),
                new PadRoute('F', 'D', 'B'),

                // X in the middle
                new PadRoute('A', 'D', 'G'),
                new PadRoute('D', 'G', 'J'),
                new PadRoute('G', 'J', 'M'),
                new PadRoute('C', 'E', 'G'),
                new PadRoute('E', 'G', 'I'),
                new PadRoute('G', 'I', 'K')
            };

        static Dictionary<char, List<PadRoute>> sm_routeMap;
        
        static Pond()
        {
            sm_padMap = new Dictionary<char, byte>(sm_padNames.Count);
            byte i = 0;
            foreach(char padName in sm_padNames)
            {
                sm_padMap[padName] = i++;
            }

            sm_routeMap = new Dictionary<char, List<PadRoute>>();
            foreach(PadRoute route in sm_routes)
            {
                if (!sm_routeMap.ContainsKey(route.End1))
                    sm_routeMap[route.End1] = new List<PadRoute>();
                sm_routeMap[route.End1].Add(route);

                if (!sm_routeMap.ContainsKey(route.End2))
                    sm_routeMap[route.End2] = new List<PadRoute>();
                sm_routeMap[route.End2].Add(route);
            }
        }

        public Task Task
        {
            get 
            {
                return m_task;
            }
            set
            {
                ClearPads();
                m_task = value;
                SetPadState(value.Red, PadState.Red);
                foreach (char green in value.Greens)
                {
                    SetPadState(green, PadState.Green);
                }
            }
        }

        private Task m_task;

        private void ClearPads()
        {
            foreach (char padName in sm_padNames)
            {
                SetPadState(padName, PadState.Empty);
            }
        }


        PadState[] m_pads = new PadState[sm_padNames.Count];

        public int GetTotalPads()
        {
            return m_pads.Length;
        }

        public void SetPad(char padName, PadState padState)
        {
            m_pads[sm_padMap[padName]] = padState;
        }

        public bool ValidRoute(char padName1, char padName2)
        {
            if (sm_routeMap.ContainsKey(padName1))
            {
                foreach (PadRoute route in sm_routeMap[padName1])
                {
                    if (route.IsRoute(padName1, padName2))
                        return true;
                }
            }
            return false;
        }

        private PadRoute GetRoute(char padName1, char padName2)
        {
            if (sm_routeMap.ContainsKey(padName1))
            {
                foreach (PadRoute route in sm_routeMap[padName1])
                {
                    if (route.IsRoute(padName1, padName2))
                        return route;
                }
            }
            return null;
        }

        private List<PadRoute> GetAllRoutes(char padName)
        {
            return sm_routeMap[padName];
        }

        public bool ValidHop(char padName1, char padName2)
        {
            PadRoute r = GetRoute(padName1, padName2);
            return r != null 
                && IsPadState(padName1, PadState.Green, PadState.Red) 
                && IsPadState(padName2, PadState.Empty) 
                && IsPadState(r.Middle, PadState.Green);
        }

        private bool IsPadState(char padName, params PadState[] padStates)
        {
            foreach (PadState p in padStates)
            {
                if (GetPadState(padName) == p)
                    return true;
            }
            return false;
        }

        public PadState GetPadState(char padName)
        {
            if (sm_padMap.ContainsKey(padName))
                return m_pads[sm_padMap[padName]];
            else
                return PadState.Empty;
        }

        private PadState SetPadState(char padName, PadState newState)
        {
            byte padIndex = sm_padMap[padName];
            PadState oldState = m_pads[padIndex];
            m_pads[padIndex] = newState;
            return oldState;
        }

        public void Hop(Hop hop)
        {
            PadRoute r = GetRoute(hop.StartPad, hop.EndPad);
            PadState ps = SetPadState(hop.StartPad, PadState.Empty);
            SetPadState(r.Middle, PadState.Empty);
            SetPadState(hop.EndPad, ps);
        }

        public void UndoHop(Hop hop)
        {
            PadRoute r = GetRoute(hop.StartPad, hop.EndPad);
            PadState ps = SetPadState(hop.EndPad, PadState.Empty);
            SetPadState(r.Middle, PadState.Green);
            SetPadState(hop.StartPad, ps);
        }

        public int GetFrogCounts(out int red, out int green)
        {
            red = green = 0;
            for (int i = 0; i < m_pads.Length; i++)
            {
                if (m_pads[i] == PadState.Red)
                    red++;
                else if (m_pads[i] == PadState.Green)
                    green++;
            }
            return red + green;
        }

        public bool IsMovementPossible()
        {
            foreach (char padName in sm_padNames)
            {
                PadState ps = GetPadState(padName);
                if (ps != PadState.Empty)
                {
                    List<char> pads = GetPossibleHops(padName);
                    if (pads.Count > 0)
                        return true;
                }
            }
            return false;
        }

        public List<char> GetPossibleHops(char padName)
        {
            List<char> possibleHops = new List<char>();
            List<PadRoute> allRoutes = GetAllRoutes(padName);
            foreach (PadRoute route in allRoutes)
            {
                if (CanHop(padName, route))
                {
                    possibleHops.Add(route.GetOppositePad(padName));
                }
            }
            return possibleHops;
        }

        private bool CanHop(char fromPad, PadRoute route)
        {
            char toPad = route.GetOppositePad(fromPad);
            return GetPadState(route.Middle) == PadState.Green 
                && GetPadState(toPad) == PadState.Empty;
        }
    }
}
