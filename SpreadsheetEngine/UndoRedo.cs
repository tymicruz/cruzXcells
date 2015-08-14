/*
 * Name: Tyler Cruz
 * ID:  11333476
 * 
 * Description: 
 * 
 * HW8
 * Implement cell color changing
 * Implement undo and redo
 * 
 * Progress towards an app similar to excel
 * Enter text into gui cell and the text will be process by the Spreadsheet Engine
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine
{
    public interface IUndoRedo
    {
        IUndoRedo Execute(Spreadsheet ss);
    }

    public class RestoreText : IUndoRedo
    {
        private string m_Text;
        private int m_CellRow;
        private int m_CellCol;

        public RestoreText(string text, int row, int col)
        {
            m_Text = text;
            m_CellRow = row;
            m_CellCol = col;
        }

        public IUndoRedo Execute(Spreadsheet ss)
        {
            //commented code below is used if we using a string for cell name instead of row and column
            //int col = m_CellName[0] - 'A';//col index of variable (provider_cell) value
            //int row = Convert.ToInt32(m_CellName.Substring(1)) - 1;//row index of variable (provider_cell) value

            Cell cell = ss.getCell(m_CellRow, m_CellCol);

            cell.Text = m_Text;//cell text is retored

            return (new RestoreText(m_Text, m_CellRow, m_CellCol));
        }
    }

    public class RestoreColor : IUndoRedo
    {
        private int m_RGB;
        private int m_CellRow;
        private int m_CellCol;

        public RestoreColor(int rgb, int row, int col)
        {
            m_RGB = rgb;
            m_CellRow = row;
            m_CellCol = col;
        }

        public IUndoRedo Execute(Spreadsheet ss)
        {
            //commented code below is used if we using a string for cell name instead of row and column
            //int col = m_CellName[0] - 'A';//col index of variable (provider_cell) value
            //int row = Convert.ToInt32(m_CellName.Substring(1)) - 1;//row index of variable (provider_cell) value

            Cell cell = ss.getCell(m_CellRow, m_CellCol);

            cell.BGColor = m_RGB;//cell color is retored

            return (new RestoreColor(m_RGB, m_CellRow, m_CellCol));
        }
    }

    public class UndoRedoCollection
    {
        private string m_Text;
        private List<IUndoRedo> m_Cmds;//commands to be executed when exec is called

        public string Text
        {
            get { return m_Text; }
        }

        public UndoRedoCollection(string text, List<IUndoRedo> commands)
        {
            m_Text = text;
            m_Cmds = commands;
        }

        public UndoRedoCollection Exec(Spreadsheet ss)
        {
            List<IUndoRedo> inverseCommands = new List<IUndoRedo>();

            foreach (IUndoRedo cmd in m_Cmds)
            {
                inverseCommands.Insert(0, cmd.Execute(ss));//push this result
            }

            //do the commands and return the opposite

            return new UndoRedoCollection(m_Text, inverseCommands);
        }
    }

    public class UndoSystem
    {
        private Stack<UndoRedoCollection> m_Undos;
        private Stack<UndoRedoCollection> m_Redos;

        public UndoSystem()
        {
            m_Redos = new Stack<UndoRedoCollection>();
            m_Undos = new Stack<UndoRedoCollection>();
        }

        public string getTopUndoName()
        {
            if (m_Undos.Count > 1)//could just call this is empty function
            {
                return m_Undos.Peek().Text;//return undo top's command name
            }
            else
            {
                return "";
            }
        }

        public string getTopRedoName()
        {
            if (m_Redos.Count > 1)//could just call the is empty functions
            {
                return m_Redos.Peek().Text;//return redo top's command name
            }
            else
            {
                return "";
            }
        }

        public void undo(Spreadsheet ss)
        {
            //don't call this function unless we have items in the m_Undos stack

            //pop from the undo stack
            //execute the undo that was popped off and push that action on to the redo stack

            //pop two things off at a time (switching between stacks determines which of the two you will use
            //think of it as undos using the odd numbers and redos using the even numbers
            m_Redos.Push(m_Undos.Pop());
            m_Redos.Push((m_Undos.Pop().Exec(ss)));//after we do this put it on the redo stack
        }

        public void redo(Spreadsheet ss)
        {
            //don't call this function unless we have items in the m_Undos stack
            //execute the redo that was popped off and push that action on to the undo stack
            m_Undos.Push(m_Redos.Pop());
            m_Undos.Push(m_Redos.Pop().Exec(ss));//after we redo it put this on the undo stack
        }

        public bool emptyUndo()
        {
            if (m_Undos.Count > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool emptyRedo()
        {
            if (m_Redos.Count > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //add a collections of "simultaneous commands to the undo stack"
        public void addUndo(string action, List<IUndoRedo> commands)
        {
            m_Undos.Push(new UndoRedoCollection(action, commands));
        }

        //call this when we need to forget what the redo stack
        //we need to call this the user changes something.
        public void clearRedo()
        {
            m_Redos.Clear();
        }
    }

}
