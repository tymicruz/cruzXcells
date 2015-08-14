/*
 * Name: Tyler Cruz
 * ID:  11333476
 * 
 * Description: 
 * 
 * HW10
 * Deal with circular references
 * 
 * the added code is in the spreadsheet.cs file within the OnTextChange event
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
using System.Xml;



namespace SpreadsheetEngine
{
    public class Spreadsheet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        readonly private int m_numRows;
        readonly private int m_numCols;

        private Cell[,] m_Cells;
        private UndoSystem m_UnRedoSystem = new UndoSystem();
        private HashSet<Cell> m_CellsToSave = new HashSet<Cell>();

        public int NumRows
        {
            get { return m_numRows; }
        }

        public int NumCols
        {
            get { return m_numCols; }
        }

        public UndoSystem UnRedoSystem
        {
            get { return m_UnRedoSystem; }
        }

        public HashSet<Cell> CellsToSave
        {
            get { return m_CellsToSave; }
        }

        public Spreadsheet(int row, int col)
        {
            m_Cells = new SSCell[row, col];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    m_Cells[i, j] = new SSCell(i, j);
                    m_Cells[i, j].PropertyChanged += OnCellChange;//if cell changes, OnCellTextChange function will be executed
                }
            }

            m_numRows = row;
            m_numCols = col;
        }

        private void cellSaving(SSCell cell)
        {
            if (cell.Text != "" && cell.Text != null)//cell has text in it
            {
                this.m_CellsToSave.Add(cell);
            }
            else if (cell.BGColor != -1)//basic white
            {
                this.m_CellsToSave.Add(cell);
            }
            else//check to see if cell is in and remove it because it is worthless
            {
                //if cell text == "" or null and BGColor is white, remove this cell if it exists
                if (this.m_CellsToSave.Contains(cell))
                {
                    this.m_CellsToSave.Remove(cell);
                }
            }
        }

        //when a cell changes in this spreadsheet, this will happen
        public void OnCellChange(object sender, PropertyChangedEventArgs e)
        {
            SSCell cell = sender as SSCell;
            string text = cell.Text;

            //add this cell to cells to be saved
            if (cell != null)
            {
                this.cellSaving(cell);
            }

            if (e.PropertyName == "Text")
            {
                //tell all providers of this cell that you no longer need them since we are starting over
                foreach (string provider in cell.providers)
                {
                    //name of sender cell that just changed
                    string cell_name = "" + (char)(cell.columnIndex + 'A') + (cell.rowIndex + 1).ToString();//ex. G20

                    int col = provider[0] - 'A';//col index of variable (provider_cell) value
                    int row = Convert.ToInt32(provider.Substring(1)) - 1;//row index of variable (provider_cell) value

                    SSCell provider_cell = getCell(row, col) as SSCell;

                    //if provider_cell has this cell as a dependent
                    if (provider_cell != null && provider_cell.dependents.Contains(cell_name))
                    {
                        //tell this provider_cell that this cell doesn't depend on it anymore
                        provider_cell.dependents.Remove(cell_name);
                    }
                }

                if (text.Length < 1)
                {
                    //empty cell
                    cell.expr = new Expression("");//reset expression;
                    cell.setValue(cell.Text);

                    if (PropertyChanged != null)//check if anyone is subscribed
                    {
                        PropertyChanged(sender, new PropertyChangedEventArgs("Text"));//spreadsheet should be notified of this change
                    }
                }
                else if (text[0] != '=')
                {
                    cell.expr = new Expression("");//reset expression;
                    cell.setValue(cell.Text);

                    if (PropertyChanged != null)//check if anyone is subscribed
                    {
                        PropertyChanged(sender, new PropertyChangedEventArgs("Text"));//spreadsheet should be notified of this change
                    }
                }
                else //deal with equation
                {
                    bool fail = false;
                    //adjust the cell's expression
                    try
                    {
                        cell.expr = new Expression(text.Substring(1));
                    }
                    catch
                    {
                        cell.expr = new Expression("");
                        cell.setValue("INVALID EQ");
                        fail = true;
                    }
                    bool bad_cell_ref = false;

                    //find variables in the table & define all variables that need to be defined
                    foreach (string key in cell.expr.m_keys)
                    {
                        if (key.Length < 2)
                        {
                            cell.setValue("!(bad cell ref)");
                            bad_cell_ref = true;
                            continue;
                        }//invalid variable, keep set at 0;

                        int col = key[0] - 'A';//col index of variable (provider_cell) value 
                        int row = -1;

                        try//check if 
                        {
                            row = Convert.ToInt32(key.Substring(1)) - 1;//row index of variable (provider_cell) value
                        }
                        catch
                        {
                            //this will make the provider_cell null and we will skip this variable
                            row = -1;
                        }

                        //provider_cell will provide (this)cell with its value
                        SSCell provider_cell = getCell(row, col) as SSCell;

                        //if this cell doesn't exist. set value to 0 & check next variable in the expression
                        if (null == provider_cell)
                        {
                            bad_cell_ref = true;
                            continue;
                        }

                        //name of cell whose text just changed
                        string cell_name = "" + (char)(cell.columnIndex + 'A') + (cell.rowIndex + 1).ToString();//ex. "B2"

                        //name of provider_cell
                        string provider_cell_name = "" + (char)(provider_cell.columnIndex + 'A') + (provider_cell.rowIndex + 1).ToString();

                        //provider cell keeps track of the cells that depend on it
                        provider_cell.dependents.Add(cell_name);

                        //cell keeps track of the cells that provide for it
                        cell.providers.Add(provider_cell_name);

                        double valx = 0;

                        try
                        {
                            //convert value string to double and define in dict
                            valx = Convert.ToDouble(provider_cell.Value);
                        }
                        catch
                        {
                            //if cell's value cannot be converted into a double
                            valx = 0;
                        }

                        cell.expr.define(key, valx);//define variable in the expression classes dictionary which lives in the Cell class
                    }

                    if (!fail)
                    {
                        //this is the value
                        if (bad_cell_ref)
                        {
                            cell.setValue("!(bad cell ref)");
                        }
                        else
                        {
                            string val = (cell.expr.evalTree()).ToString();
                            //set value of the cell
                            cell.setValue(val);//this will trigger a value propertychange
                        }
                    }

                    if (PropertyChanged != null)//if someone subscribed
                    {
                        PropertyChanged(sender, new PropertyChangedEventArgs("Text"));//tell the world(UI spread sheet) that this cell's text just changed
                    }
                }
            }
            else if (e.PropertyName == "Value")
            {

                string cell_name = "" + (char)(cell.columnIndex + 'A') + (cell.rowIndex + 1).ToString();

                //if you directly depend on yourself
                if (cell.dependents.Contains(cell_name))
                {
                    //remove from itself from the dependents
                    cell.dependents.Remove(cell_name);
                    //reset cell name and we will come back to this function without the self ref
                    cell.setValue("!(self reference)");
                }
                else if(hasCircularRef(cell, cell_name))
                {
                    cell.setValue("!(circular reference)");


                    //foreach (string dependent in cell.dependents)
                    //{
                    //    int col = dependent[0] - 'A';//col of dependent cell
                    //    int row = Convert.ToInt32(dependent.Substring(1)) - 1;//row of dependent cell

                    //    SSCell dependent_cell = getCell(row, col) as SSCell;

                    //    //redefine the variable in the dependent cell's dictionary
                    //    dependent_cell.expr.define(cell_name, 0);

                    //    //reset value 
                    //    //if (!hasCircularRef(dependent_cell, dependent))
                    //    //{
                    //        dependent_cell.setValueNoTrigger("circular reference");
                    //    //}

                    //    //if someone is subscribed tell them about this change (spreadsheet is subscribed to this event)
                    //    if (PropertyChanged != null)
                    //    {
                    //        //tell the world(spread sheet) that the dependent cell needs to be updated
                    //        PropertyChanged(dependent_cell, new PropertyChangedEventArgs("Value"));
                    //        //notice we pass in the dependent cell and not "sender"
                    //    }
                    //}
                    //on value change: tell all dependent cells about the change
        
                }
                else
                {
                    //on value change: tell all dependent cells about the change
                    foreach (string dependent in cell.dependents)//notify all that depend on you
                    {
                        //the name of the cell that is notifying its dependents of the change
                        //string cell_name = "" + (char)(cell.columnIndex + 'A') + (cell.rowIndex + 1).ToString();

                        int col = dependent[0] - 'A';//col of dependent cell
                        int row = Convert.ToInt32(dependent.Substring(1)) - 1;//row of dependent cell

                        double new_value = 0;

                        //try-catch: check to see if cell's val can be converted into a double 
                        try
                        {
                            new_value = Convert.ToDouble(cell.Value);
                        }
                        catch
                        {
                            //if incompatible with doubles, set to 0
                            new_value = 0;
                        }

                        //this is the dependent cell
                        SSCell dependent_cell = getCell(row, col) as SSCell;

                        //redefine the variable in the dependent cell's dictionary
                        dependent_cell.expr.define(cell_name, new_value);

                        //reset value 
                        dependent_cell.setValue(dependent_cell.expr.evalTree().ToString());

                        //if someone is subscribed tell them about this change (spreadsheet is subscribed to this event)
                        if (PropertyChanged != null)
                        {
                            //tell the world(spread sheet) that the DEPENDENT cell needs to be updated
                            PropertyChanged(dependent_cell, new PropertyChangedEventArgs("Value"));
                            //notice we pass in the dependent cell and not "sender"
                        }
                    }

                }
            }
            else if (e.PropertyName == "BGColor")
            {
                //spreadsheet has been notified, but now we need spreadsheet to tell the things that subscribe to it what has just happened
                //the spreadsheet can't just keep the color change a secret to itself
                if (PropertyChanged != null)
                {
                    PropertyChanged(sender, new PropertyChangedEventArgs("BGColor"));
                }
            }
        }

        private bool hasCircularRef(Cell cell, string cell_name)
        {
            List<bool> falses = new List<bool>();

            if (cell.dependents.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (string dependent in cell.dependents)
                {
                    int col = dependent[0] - 'A';//col of dependent cell
                    int row = Convert.ToInt32(dependent.Substring(1)) - 1;//row of dependent cell
                  
                    if (dependent != cell_name)
                    {
                        falses.Add(hasCircularRef(this.getCell(row, col), cell_name));
                    }
                    else//the original cell depends on this cell
                    {

                        //SSCell dependent_cell = this.getCell(row, col) as SSCell;
                        //.setValueNoTrigger("circ ref");
                        //cell.dependents.Remove(cell_name);
                        return true;
                    }
                }

                foreach (bool f in falses)
                {
                    if(f == true)
                    {
                        return true; 
                    }
                }

                return false;
            }
        }

        public void loadSpreadSheet(string name)
        {
            //current row, col, and color 
            int crow = 0;
            int ccol = 0;
            int ccolor = -1;

            //used to get text from text element
            bool text_hit = false;

            using (XmlReader xreader = XmlReader.Create(name))
            {
                this.reset();

                while (xreader.Read())
                {
                    switch (xreader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xreader.Name == "cell")
                            {
                                crow = Convert.ToInt32(xreader.GetAttribute("row"));
                                ccol = Convert.ToInt32(xreader.GetAttribute("col"));
                            }
                            if (xreader.Name == "background")
                            {
                                ccolor = Convert.ToInt32(xreader.GetAttribute("color"));
                                this.getCell(crow, ccol).BGColor = ccolor;
                            }
                            if (xreader.Name == "text")
                            {
                                //xreader.MoveToContent();
                                //sheet.getCell(crow, ccol).Text = xreader.Value;
                                text_hit = true;
                            }
                            break;
                        //I feel like this will mess up if text as attributes
                        case XmlNodeType.Text:
                            if (text_hit)
                            {
                                //this text should always be hit right after we see the text element
                                this.getCell(crow, ccol).Text = xreader.Value;
                                text_hit = false;//reset text_hit variable
                            }
                            break;
                    }
                }

                xreader.Close();
            }
        }

        public void saveSpreadSheet(string name)
        {
            //name = name of file we are saving to
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            //settings.NewLineChars = "\n";

            using (XmlWriter xwriter = XmlWriter.Create(name, settings))
            {

                //this implementation only supports saving a single spreadsheet
                xwriter.WriteStartElement("spreadsheet");
                foreach (Cell cell in this.CellsToSave)
                {
                    xwriter.WriteStartElement("cell");
                    xwriter.WriteAttributeString("row", cell.rowIndex.ToString());
                    xwriter.WriteAttributeString("col", cell.columnIndex.ToString());
                    {
                        if (cell.Text != "")
                        {
                            xwriter.WriteStartElement("text");
                            //xwriter.WriteAttributeString("font", "tnr");
                            xwriter.WriteValue(cell.Text);
                            xwriter.WriteEndElement();//end text element
                        }

                        if (cell.BGColor != -1)//-1 is white which is the default color
                        {
                            xwriter.WriteStartElement("background");
                            xwriter.WriteAttributeString("color", cell.BGColor.ToString());
                            xwriter.WriteEndElement();//end background element
                        }
                    }
                    xwriter.WriteEndElement();//end cell element
                }

                xwriter.WriteEndElement();//end spreadsheet element
                xwriter.Close();
            }
        }

        public void reset()
        {
            try//exception will be thrown when cell is removed while iterating, but just keep doing it until you do it for all cells
            {
                foreach (Cell cell in (this.CellsToSave))
                {
                    cell.Text = "";//this will trigger cell to be removed from the CellsToSave HashSet
                    cell.BGColor = -1;
                }
            }
            catch
            {
                this.reset();
            }

            this.m_UnRedoSystem = new UndoSystem();

        }

        public Cell getCell(int row, int col)
        {
            if ((row >= 0 && row < m_numRows) && (col >= 0 && col < m_numCols))
            {
                return (this.m_Cells[row, col]);
            }
            else
            {
                return null;
            }
        }

        //SSCell class is a class within the Spreadsheet class
        private class SSCell : Cell
        {
            public Expression expr = new Expression("");

            public SSCell(int row, int col)
                : base(row, col)
            {

            }

            public void setValue(string value)
            {
                this.Value = value;
            }

            //public void setValueNoTrigger(string value)
            //{
            //    this.ValueNoTrigger = value;
            //}
        }
    }



}
