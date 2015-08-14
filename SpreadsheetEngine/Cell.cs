/*
 * Name: Tyler Cruz
 * ID:  11333476
 * 
 * Description: 
 * 
 * HW10
 * Deal with circular references
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
using System.ComponentModel;

namespace SpreadsheetEngine
{
    public abstract class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public HashSet<string> dependents = new HashSet<string>();//each cell has list of names of the cells which depend on it
        public HashSet<string> providers = new HashSet<string>();//each cell has a list of names of the cells which provide for it

        readonly private int m_RowIndex;//(readonly)cells should never move, once set they're set, their position is set.
        readonly private int m_ColumnIndex;//(readonly)cells should never move, once set they're set, their position is set.

        private string m_Text;//allow
        private string m_Value;//protected: allow SSCell to see set this variable
        private int m_BGColor;

        public Cell(int row, int col)
        {
            m_RowIndex = row;
            m_ColumnIndex = col;
            m_BGColor = -1;//-1 is white

            m_Text = "";
            m_Value = m_Text;
        }

        public int rowIndex
        {
            get { return m_RowIndex; }

        }

        public int columnIndex
        {
            get { return m_ColumnIndex; }

        }

        public string Text
        {
            get { return m_Text; }
            set
            {
                if (value != m_Text)
                {
                    m_Text = value;

                    //trigger event
                    //OnPropertyChanged("Text");

                    if (null != PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                    }
                }
            }
        }

        public string Value
        {
            get { return m_Value; }

            protected set//only allow SS to set this value
            {
                if (value != m_Value)
                {
                    m_Value = value;

                    if (null != PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                }
            }
        }

        //public string ValueNoTrigger
        //{
        //    get { return m_Value; }

        //    protected set//only allow SS to set this value
        //    {
        //        if (value != m_Value)
        //        {
        //            m_Value = value;

        //            //if (null != PropertyChanged)
        //            //{
        //            //    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        //            //}
        //        }
        //    }
        //}

        public int BGColor
        {
            get { return m_BGColor; }

            set
            {
                if (value != m_BGColor)
                {
                   m_BGColor = value;

                    if(null != PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BGColor"));
                    }
                }
            }
        }

        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;

        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
    }
}
