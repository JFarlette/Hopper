using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HopperEngine
{
    public class Task
    {
        public enum Level
        {
            Beginner,
            Intermediate,
            Advanced,
            Expert
        }

        private static Level LevelFromChar(char l)
        {
            switch (l)
            {
                case 'B': return Level.Beginner; 
                case 'I': return Level.Intermediate; 
                case 'A': return Level.Advanced; 
                case 'E': return Level.Expert; 
            }
            throw new ArgumentException("Invalid Level abreviation character: " + l);
        }


        public Task(string details)
        {
            // Convert to uppercase and strip out spaces
            string cleaned = Regex.Replace(details.ToUpper(), @" ", "", RegexOptions.None);

            Match m = Regex.Match(cleaned, "^(?<number>[0-9]+):(?<level>[BIAE]):(?<positions>[A-M].+):(?<solution>.+)$");
            if (!m.Success)
                throw new ArgumentException("Invalid task specified: " + details);

            Name = m.Groups["number"].Value;
            string level = m.Groups["level"].Value;
            string positions = m.Groups["positions"].Value;
            string solutions = m.Groups["solution"].Value;

            Level difficulty = LevelFromChar(level[0]);
            char red = positions[0];
            char[] greens = positions.Substring(1).ToCharArray();

            List<Hop> solution = new List<Hop>();

            // Split up the solution into separate moves and parse each
            string[] moves = solutions.Split(',');
            foreach (string move in moves)
            {
                string[] steps = move.Split(new char[] { '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
                string start = steps[0];
                if (start.Length != 1)
                    throw new ArgumentException("Invalid task specified - move start invalid: " + move );
                char startPad = start[0];
                for (int i = 1; i < steps.Length; i++)
                {
                    string next = steps[i];
                    if (next.Length != 1)
                        throw new ArgumentException("Invalid task specified - move next invalid: " + move);
                    char endPad = next[0];
                    Hop hop = new Hop(startPad, endPad);
                    solution.Add(hop);
                    startPad = endPad;
                }
            }

            Difficulty = difficulty;
            Red = red;
            Greens = greens;
            Solution = solution;
            Number = sm_taskNumber++;
        }
        

        public Level Difficulty;
        public int Number;
        public char Red;
        public char[] Greens;
        public string Name;

        public List<Hop> Solution;

        private static int sm_taskNumber = 0;

    }

    public class Tasks
    {
        public Tasks()
        {
            m_tasks.Add(new Task("1 : B : I A D G : I-E, A-G, E-I"));
            m_tasks.Add(new Task("2 : B : F A C D E : A-G, F-H, C-G, H-F"));
            m_tasks.Add(new Task("3 : B : G A D E F : G-C, A-G, F-H, C-M"));
            m_tasks.Add(new Task("4 : B : H A F G I : A-K, H-F, K-G, F-H"));
            m_tasks.Add(new Task("5 : B : A G H I L : L-F, A-K, H-F, K-A"));
            m_tasks.Add(new Task("6 : B : A B D F K L : A-C, K-A/G, L-B, C-A"));
            m_tasks.Add(new Task("7 : B : G A D F H I : A-K, G-A, K-G, H-F, A-K"));
            m_tasks.Add(new Task("8 : B : G A D E F J : G-C, A-G, F-H, C-M/G"));
            m_tasks.Add(new Task("9 : B : E A D F H I J : A-K, H-L/F, K-A/G, E-I"));
            m_tasks.Add(new Task("10 : B : B A D F G I M : F-H, A-G, I-E, M-C/G, B-L"));
            m_tasks.Add(new Task("11: I : J A D E I L : L - F/B, A - C/G, J - D"));
            m_tasks.Add(new Task("12 : I : L A B E F H I : A-C, H-B, C-A/K/G, L-B"));
            m_tasks.Add(new Task("13 : I : F A B D I K L : K-G, A-C, L-B, C-A/G, F-H"));
            m_tasks.Add(new Task("14 : I : K A B D F J : A-G, K-A, J-D, B-F, A-K"));
            m_tasks.Add(new Task("15 : I : J A D E G H I L : G-C, A-G, J-D, C-M/K/G, D-J"));
            m_tasks.Add(new Task("16 : I : D B E F G H I J L : G-C/M, L-H, M-C/A/K/G, D-J"));
            m_tasks.Add(new Task("17 : I : J A B D E F G H I L : A-C, G-K/A/G, E-I, C-M/K/G, J-D"));
            m_tasks.Add(new Task("18 : I : I A B D E F J : A-C, F-B/H, C-M/G, I-E"));
            m_tasks.Add(new Task("19 : I : D A B E F I J L : A-C, L-H/B, C-A/K/G, D-J")); 
            m_tasks.Add(new Task("20 : I : E B D F G I J L : G-M/K/A, B-F, A-K/G, E-I")); 
            m_tasks.Add(new Task("21 : A : I A D F G J L M : I-E, A-G, J-D, M-K/A/G, E-I"));
            m_tasks.Add(new Task("22 : A : I A B D  E F G J M : A-C, F-H/B, C-A/G, I-E, M-G, E-I"));
            m_tasks.Add(new Task("23 : A : L A B D E F H I M : A-K/G, E-I, M-C/A/G, L-F/H"));
            m_tasks.Add(new Task("24 : A : F A C D E G I J K M : F-H, A-G, J-D, K-G, D-J, C-G, H-F, M-G, F-H"));
            m_tasks.Add(new Task("25 : A : J A B D E F H I L : A-C, H-B, C-A/K, L-F, K-A/G, J-D"));
            m_tasks.Add(new Task("26 : A : J A B D E F G I : A-C, G-K/A/G, J-D, C-G, D-J"));
            m_tasks.Add(new Task("27 : A : B A D E F H I K : F-L, K-M/C/G, B-L, A-G, L-B"));
            m_tasks.Add(new Task("28 : A : G A D E F I J K : G-C, A-G, F-H, K-G, H-L/B, C-A"));
            m_tasks.Add(new Task("29 : A : J A B C D E F I K : A-G, J-D, K-G, E-I, C-A/K/G, D-J"));
            m_tasks.Add(new Task("30 : A : A C D E F J L M : A-K, M-G, D-J, C-G, L-H/F, K-A"));
            m_tasks.Add(new Task("31 : E : F A B D E G J K L M : F-H, A-C/G, H-F, M-G, D-J, K-M/G, F-H"));
            m_tasks.Add(new Task("32 : E : G A C D E F H I J K : G-M, K-G, D-J, C-G, J-D, A-G, M-C, F-H, C-M"));
            m_tasks.Add(new Task("33 : E : G A D E F J L M : A-K, G-A, M-G, E-I, K-G, L-B, A-C"));
            m_tasks.Add(new Task("34 : E : G A D E F H I L M : A-K, G-A, K-G, E-I, M-K/G, H-F, A-K"));
            m_tasks.Add(new Task("35 : E : I A B D E F G H J L M : A-C, H-B, I-E, C-A/G, J-D, M-K/A/G, E-I"));
            m_tasks.Add(new Task("36 : E : G A C D E F H I L : A-K/M, G-K, C-G, D-J, M-G, H-F, K-A"));
            m_tasks.Add(new Task("37 : E : G A B C D E F I J K : G-M, A-G, F-H, K-G, B-L, C-G, L-B, M-C/A"));
            m_tasks.Add(new Task("38:E:ABDFGHJKM:B-L,M-G,D-J,K-M/G,A-K,H-F,K-A"));
            m_tasks.Add(new Task("39:E:MABCDEFHIKL:K-G,D-J,M-K,C-M/G,F-H,A-C/G,H-F,K-A"));
            m_tasks.Add(new Task("40:E:FABCDEGIJKLM:F-H, A-G, J-D, K-G, E-I, C-A/G, H-F, M-K/G, F-H"));                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
        }

        public Task GetFirstTask(Task.Level difficulty)
        {
            foreach (Task t in m_tasks)
            {
                if (t.Difficulty == difficulty)
                    return t;
            }

            throw new ArgumentOutOfRangeException("difficulty", "No task with difficulty == " + difficulty.ToString());
        }

        public Task GetNextTask(Task task)
        {
            int next = task.Number + 1;
            if (next >= m_tasks.Count) next = 0;
            return m_tasks[next];
        }

        public Task GetTask(int taskNumber)
        {
            return m_tasks[Math.Min(Math.Max(0, taskNumber), m_tasks.Count-1)];
        }

        internal bool IsValidTask(int taskNumber)
        {
            return 0 <= taskNumber && taskNumber < m_tasks.Count;
        }

        public int Count { get { return m_tasks.Count; } }
        
        private List<Task> m_tasks = new List<Task>();

        public List<Task> TaskList
        {
          get { return m_tasks; }
        }
    }

}
