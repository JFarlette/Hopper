using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HopperEngine
{
    public class HopStack 
    {
        public void Push(Hop hop)
        {
            if (m_index >= -1 && m_index < m_hopList.Count - 1)
            {
                // Drop any outstanding redos
                List<Hop> oldList = m_hopList;
                m_hopList = new List<Hop>();
                for (int i = 0; i <= m_index; i++)
                {
                    m_hopList.Add(oldList[i]);
                }
            }
            m_hopList.Add(hop);
            m_index += 1;
        }
        public Hop Undo()
        {
            return m_hopList[m_index--];
        }
        public Hop Redo()
        {
            return m_hopList[++m_index];
        }

        List<Hop> m_hopList = new List<Hop>();

        int m_index = -1;

        public IEnumerable Undos()
        {
            for (int i = 0; i <= m_index; i++)
            {
                yield return m_hopList[i];
            }
        }

        public IEnumerable Redos()
        {
            for (int i = m_index + 1; i < m_hopList.Count; i++)
            {
                yield return m_hopList[i];
            }
        }

        public bool HasUndo()
        {
            return m_index > -1;
        }
        public bool HasRedo()
        {
            return m_index < m_hopList.Count - 1;
        }
    }
}
