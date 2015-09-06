using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HopperEngine;
using System.Diagnostics;

namespace Hopper
{
    class Program
    {
        static string sm_Help =
            "Commands:\n" +
            "\n" +
            "   (S)elect <pad>        Selects a pad; pad must contain a frog.\n" +
            "   (J)umpto <pad>        Jump the selected frog to the specified pad; must be an empty pad with a green frog on the pad between the two pads.\n" +
            "   (T)ask <#>            Switches to the specified task number.\n" +
            "   (U)ndo                Undoes the last jump; must have been at least one jump already completed.\n" +
            "   (R)edo                Redoes the last jump; must be at least one jump ondone already.\n" +
            "   (D)emo                Demo the solution.\n" +
            "   (N)ext                Skips to next task.\n" +
            "   (Q)uit                Bye bye!!";

        static GameManager sm_manager = new GameManager(new GameManager.GameStateChangedDelgate(GameStateChanged));

        static void DrawHeader()
        {
            Console.Write(String.Format(@"Task: {0} ({1} of {2})    Difficulty:  ", sm_manager.GetTaskName(), sm_manager.GetTaskNumber()+1, sm_manager.GetTotalTasks()));
            Task.Level difficulty = sm_manager.GetTaskLevel();
            switch (difficulty)
            {
                case Task.Level.Beginner: Console.ForegroundColor = ConsoleColor.Green; break;
                case Task.Level.Intermediate: Console.ForegroundColor = ConsoleColor.Magenta; break;
                case Task.Level.Advanced: Console.ForegroundColor = ConsoleColor.Cyan; break;
                case Task.Level.Expert: Console.ForegroundColor = ConsoleColor.Red; break;
            }
            Console.Write(String.Format("{0}", sm_manager.GetTaskLevel()));
            Console.ResetColor();
            Console.Write("  State: ");
            GameManager.GameState gs = sm_manager.GetGameState();
            switch (gs)
            {
                case GameManager.GameState.Playing: Console.ForegroundColor = ConsoleColor.Green; break;
                case GameManager.GameState.Lost: Console.ForegroundColor = ConsoleColor.Red; break;
                case GameManager.GameState.Won: Console.ForegroundColor = ConsoleColor.Yellow; break;
            }
            Console.WriteLine("{0}", gs.ToString());
            Console.ResetColor();
        }

        static void DrawPondInColor()
        {
                Console.Write(@"    "); RenderPadInColor('A'); Console.Write("-----"); RenderPadInColor('B'); Console.Write("-----"); RenderPadInColor('C'); Console.WriteLine();
            Console.WriteLine(@"        \     / | \     /  ");
            Console.WriteLine(@"      |  \   /  |  \   /  |");
            Console.WriteLine(@"      |   \ /   |   \ /   |");
                 Console.Write("      |  "); RenderPadInColor('D'); Console.Write("  |  "); RenderPadInColor('E'); Console.Write("  |"); Console.WriteLine();
            Console.WriteLine(@"      |   / \   |   / \   |");
            Console.WriteLine(@"      |  /   \  |  /   \  |");
            Console.WriteLine(@"        /     \ | /     \  ");
                Console.Write(@"    "); RenderPadInColor('F'); Console.Write("-----"); RenderPadInColor('G'); Console.Write("-----"); RenderPadInColor('H'); Console.WriteLine();
            Console.WriteLine(@"        \     / | \     /  ");
            Console.WriteLine(@"      |  \   /  |  \   /  |");
            Console.WriteLine(@"      |   \ /   |   \ /   |");
                Console.Write(@"      |  "); RenderPadInColor('I'); Console.Write("  |  "); RenderPadInColor('J'); Console.Write("  |"); Console.WriteLine();
            Console.WriteLine(@"      |   / \   |   / \   |");
            Console.WriteLine(@"      |  /   \  |  /   \  |");
            Console.WriteLine(@"        /     \ | /     \  ");
                Console.Write(@"    "); RenderPadInColor('K'); Console.Write("-----"); RenderPadInColor('L'); Console.Write("-----"); RenderPadInColor('M'); Console.WriteLine();
        }

        static void RenderPadInColor(char padId)
        {
            Pad p = sm_manager.GetPad(padId);
            StringBuilder sb = new StringBuilder(5);
            char selectOpen;
            char selectClose;
            switch (p.Selection)
            {
                case PadSelection.Selected: selectOpen = '['; selectClose = ']'; break;
                case PadSelection.Option: selectOpen = '<'; selectClose = '>'; Console.ForegroundColor = ConsoleColor.Yellow; break;
                default: selectOpen = selectClose = ' '; break;
            }
            sb.Append(selectOpen);
            sb.Append(' ');
            sb.Append(p.ID);
            switch (p.State)
            {
                case PadState.Green: Console.ForegroundColor = ConsoleColor.Green;  break;
                case PadState.Red: Console.ForegroundColor = ConsoleColor.Red; break;
            }
            sb.Append(' ');
            sb.Append(selectClose);

            Console.Write(sb.ToString());

            Console.ResetColor();
        }


        // Stack should be managed by HopperEngine
        static void DrawStack()
        {
            Console.Write("[ ");
            foreach (Hop hop in sm_manager.HopStack.Undos())
            {
                Console.Write("({0} <- {1}) ", hop.StartPad, hop.EndPad);
            }
            Console.Write("] ");
            foreach (Hop hop in sm_manager.HopStack.Redos())
            {
                Console.Write("({0} -> {1}) ", hop.StartPad, hop.EndPad);
            }
            Console.WriteLine();
        }
       
        static string RenderPad(char padId)
        {
            Pad p = sm_manager.GetPad(padId);
            StringBuilder sb = new StringBuilder(5);
            char selectOpen;
            char selectClose;
            switch (p.Selection)
            {
                case PadSelection.Selected: selectOpen = '['; selectClose = ']'; break;
                case PadSelection.Option: selectOpen = '<'; selectClose = '>'; break;
                default: selectOpen = selectClose = ' '; break;
            }
            sb.Append(selectOpen);
            sb.Append(p.ID);
            sb.Append(':');
            char st;
            switch (p.State)
            {
                case PadState.Green: st = 'G'; break;
                case PadState.Red: st = 'R'; break;
                default: st = ' '; break;
            }
            sb.Append(st);
            sb.Append(selectClose);
            return sb.ToString();
        }

        static void RenderConsole()
        {
            Console.Clear();
            Console.WriteLine();
            DrawHeader();
            Console.WriteLine();
            DrawPondInColor();
            Console.WriteLine();
            DrawStack();
            Console.WriteLine();
            ShowHelp();
            Console.WriteLine();
            Console.Write("?>");
        }

        static void Main(string[] args)
        {
            RenderConsole();

            bool done = false;
            do
            {
                string line = Console.ReadLine().ToUpper();
                bool handled = false;
                if (line.StartsWith("Q"))
                {
                    done = handled = true;
                }
                else if (line.StartsWith("S"))
                {
                    string[] s = line.Split(' ');
                    if (s.Length == 2)
                    {
                        char padName = s[1][0];
                        handled = sm_manager.SelectPad(padName) == GameManager.SelectionResult.Selected;
                    }
                }
                else if (line.StartsWith("J"))
                {
                    string[] s = line.Split(' ');
                    if (s.Length == 2)
                    {
                        char padName = s[1][0];
                        handled = sm_manager.JumpTo(padName) == GameManager.JumpResult.Jumped;
                    }
                }
                else if (line.StartsWith("T"))
                {
                    string[] s = line.Split(' ');
                    if (s.Length == 2)
                    {
                        try
                        {
                            int taskNum = Convert.ToInt32(s[1]);
                            handled = sm_manager.SetTask(taskNum - 1);
                        }
                        catch (Exception) { }
                    }
                }
                else if (line.StartsWith("U"))
                {
                    handled = sm_manager.UndoHop();
                }
                else if (line.StartsWith("R"))
                {
                    handled = sm_manager.RedoHop();
                }
                else if (line.StartsWith("D"))
                {
                    sm_manager.ShowSolution();
                    handled = true;
                }
                else if (line.StartsWith("N"))
                {
                    handled = sm_manager.NextTask();
                }
                
                if (!handled)
                {
                    Console.Beep();
                    RenderConsole();
                }
            }
            while (!done);

            Console.WriteLine("Thanks for playing!!!");
        }

        static void GameStateChanged(GameManager.GameState newState)
        {
            RenderConsole();
            if (newState == GameManager.GameState.Lost)
            {
                Console.Beep(200, 800);
            }
            else if (newState == GameManager.GameState.Won)
            {
                Console.Beep();
                Console.Beep();
                Console.Beep();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine(sm_Help);
        }

    }
}

