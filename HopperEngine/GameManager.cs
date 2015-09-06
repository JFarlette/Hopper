using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace HopperEngine
{
    public class GameManager
    {
        public enum GameState
        {
            NotStarted,
            Playing,
            Lost,
            Won,
            Invalid
        }

        public delegate void GameStateChangedDelgate(GameState newGameState);

        private GameState m_gameState = GameState.NotStarted;

        private Pond m_pond = new Pond();

        private Tasks m_tasks = new Tasks();

        private GameStateChangedDelgate m_gsc = null;

        public GameManager(GameStateChangedDelgate gsc = null, ShowHopDelegate sh = null)
        {
            m_pond.Task = m_tasks.GetFirstTask(Task.Level.Beginner);
            m_gsc = gsc;
            m_showHop = sh;
            UpdateGameState(false);
        }

        private void UpdateGameState(bool invokeDelgate = true)
        {
            GameState oldState = m_gameState;

            int red, green;
            int total = m_pond.GetFrogCounts(out red, out green);

            if (total == 0)
            {
                m_gameState = GameState.NotStarted;
            }
            else if (red == 1 && green == 0)
            {
                m_gameState = GameState.Won;
            }
            else if (!m_pond.IsMovementPossible())
            {
                m_gameState = GameState.Lost;
            }
            else if ((oldState == GameState.Playing && m_gameState == GameState.NotStarted)
            || (oldState == GameState.Lost && m_gameState == GameState.Playing)
            || (oldState == GameState.Lost && m_gameState == GameState.Won))
            {
                m_gameState = GameState.Invalid;
            }
            else
            {
                m_gameState = GameState.Playing;
            }

            if (m_gsc != null && invokeDelgate) 
                m_gsc(m_gameState);
        }

        public GameState GetGameState()
        {
            return m_gameState;
        }

        public int GetTaskNumber()
        {
            return m_pond.Task.Number;
        }

        public string GetTaskName()
        {
            return m_pond.Task.Name;
        }

        public Task.Level GetTaskLevel()
        {
            return m_pond.Task.Difficulty;
        }

        public Task.Level GetTaskLevel(int taskNumber)
        {
            return m_tasks.GetTask(taskNumber).Difficulty;
        }

        public string GetTaskName(int taskNumber)
        {
            return m_tasks.GetTask(taskNumber).Name;
        }

        private PadSelection GetPadSelection(char padName)
        {
            if (m_selectedPad == '\0') 
                return PadSelection.None;
            else if (padName == m_selectedPad)
                return PadSelection.Selected;
            else if (m_possibleHops.IndexOf(padName) >= 0)
                return PadSelection.Option;
            else
                return PadSelection.None;
        }

        public Pad GetPad(char padName)
        {
            return new Pad(padName, m_pond.GetPadState(padName), GetPadSelection(padName));
        }

        public enum SelectionResult
        {
            Selected,
            PadEmpty
        }

        public SelectionResult SelectPad(char padName)
        {
            SelectionResult result = SelectionResult.PadEmpty;
            if (m_pond.GetPadState(padName) != PadState.Empty)
            {
                m_selectedPad = padName;
                m_possibleHops = m_pond.GetPossibleHops(padName);
                result = SelectionResult.Selected;
                UpdateGameState();
            }
            return result;
        }

        private char m_selectedPad;
        private List<char> m_possibleHops;


        public enum JumpResult
        {
            Jumped,
            NoSelection,
            NotPossible
        }

        public JumpResult JumpTo(char jumpToPad)
        {
            if (m_selectedPad == '\0')
                return JumpResult.NoSelection;
            else if (m_possibleHops.IndexOf(jumpToPad) == -1)
                return JumpResult.NotPossible;
            else
            {
                Hop hop = new Hop(m_selectedPad, jumpToPad);
                m_pond.Hop(hop);
                m_hopStack.Push(hop);
                ShowHop(hop);
                SelectPad(jumpToPad);
                // UpdateGameState();
                return JumpResult.Jumped;
            }
        }

        private void ShowHop(Hop hop)
        {
            if (m_showHop != null)
            {
                m_showHop(hop);
            }
        }

        public delegate void ShowHopDelegate(Hop hop);
        private ShowHopDelegate m_showHop = null;

        HopStack m_hopStack = new HopStack();

        public bool NextTask()
        {
            m_pond.Task = m_tasks.GetNextTask(m_pond.Task);
            m_selectedPad = '\0';
            m_hopStack = new HopStack();
            UpdateGameState();
            return true;
        }

        public HopStack HopStack 
        {
            get
            {
                return m_hopStack;
            }
        }

        public bool UndoHop()
        {
            bool result = false;
            if (result = m_hopStack.HasUndo())
            {
                Hop hop = m_hopStack.Undo();
                m_pond.UndoHop(hop);
                SelectPad(hop.StartPad);
            }
            return result;
        }

        public bool RedoHop()
        {
            bool result = false;
            if (result = m_hopStack.HasRedo())
            {
                Hop hop = m_hopStack.Redo();
                m_pond.Hop(hop);
                SelectPad(hop.EndPad);
            }
            return result;
        }


        public bool SetTask(int taskNumber)
        {
            if (!m_tasks.IsValidTask(taskNumber))
                return false;
            Task task = m_tasks.GetTask(taskNumber);
            if (m_pond.Task != task)
            {
                m_pond.Task = task;
                m_selectedPad = '\0';
                m_hopStack = new HopStack();
                UpdateGameState();
            }
            return true;
        }

        public int GetTotalTasks()
        {
            return m_tasks.Count;
        }

        public List<Task> TaskList
        {
            get { return m_tasks.TaskList;  }
        }

        #region This does not belong here.  It does not work in WPF application and it can't be moved to the console application as is.
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        // Need keys for S, J, A-M, <Enter>, & <Space>
        const byte VK_A = 0x41;
        const byte VK_B = 0x42;
        const byte VK_C = 0x43;
        const byte VK_D = 0x44;
        const byte VK_E = 0x45;
        const byte VK_F = 0x46;
        const byte VK_G = 0x47;
        const byte VK_H = 0x48;
        const byte VK_I = 0x49;
        const byte VK_J = 0x4A;
        const byte VK_K = 0x4B;
        const byte VK_L = 0x4C;
        const byte VK_M = 0x4D;

        const byte VK_S = 0x53;

        const byte VK_SPACE = 0x20;
        const byte VK_RETURN = 0x0D;

        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private byte ConvertToKey(char padId)
        {
            byte key = 0;
            switch (padId)
            {
                case 'A': key = VK_A; break;
                case 'B': key = VK_B; break;
                case 'C': key = VK_C; break;
                case 'D': key = VK_D; break;
                case 'E': key = VK_E; break;
                case 'F': key = VK_F; break;
                case 'G': key = VK_G; break;
                case 'H': key = VK_H; break;
                case 'I': key = VK_I; break;
                case 'J': key = VK_J; break;
                case 'K': key = VK_K; break;
                case 'L': key = VK_L; break;
                case 'M': key = VK_M; break;
            }

            return key;
        }

        private void KeysForHop(Hop hop)
        {
            // Select command
            keybd_event(VK_S, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_S, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            keybd_event(VK_SPACE, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_SPACE, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            byte selectKey = ConvertToKey(hop.StartPad);

            keybd_event(selectKey, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(selectKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            Thread.Sleep(2000);

            // Jump command
            keybd_event(VK_J, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_J, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            keybd_event(VK_SPACE, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_SPACE, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            byte jumpKey = ConvertToKey(hop.EndPad);

            keybd_event(jumpKey, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(jumpKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_RETURN, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            Thread.Sleep(2000);
        }


        private void RunDemo()
        {
            foreach (Hop hop in m_pond.Task.Solution)
            {
                KeysForHop(hop);
            }
        }

        public void ShowSolution()
        {
            SetTask(m_pond.Task.Number);

            Thread t = new Thread(new ThreadStart(RunDemo));
            t.Start();
        }
        #endregion
    }
}
