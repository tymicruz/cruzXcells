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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetEngine;
using System.Xml;

namespace Cruz_Tyler_322_HW_10
{
    public partial class Form1 : Form
    {
        //this sheet contains all of the logic
        public Spreadsheet sheet = new Spreadsheet(50, 26);//rows, columns

        public Form1()
        {
            InitializeComponent();
            sheet.PropertyChanged += SSChangeHandler;//connects logic of sheet to the UI
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setButtons();
            dataGridView1.Columns.Clear();
            dataGridView1.RowHeadersWidth = 70;//43 is default/ make full number visible for everyone

            //don't allow any cols or rows to be created in the gui, because logic is only good for the fixed gui
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = false;

            //create GUI
            //add 26 columns
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)(i + 65);

                //add a column with add columnName and header text equal to a letter
                dataGridView1.Columns.Add(letter.ToString(), letter.ToString());
            }

            //add 50 rows
            for (int i = 1; i <= 50; i++)
            {
                dataGridView1.Rows.Add(1);//add one row

                //add row labels with 'i'
                dataGridView1.Rows[(i - 1)].HeaderCell.Value = i.ToString();

            }

            // Set the selection background color for all the cells.
            dataGridView1.DefaultCellStyle.BackColor = Color.White;

            //set color of cells when they are selected
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
        }

        //when an event happens in the spreadsheet, this will be called
        //when any sell changes, events will be triggered
        public void SSChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            Cell cell = sender as Cell;

            if (cell != null)
            {
                if (e.PropertyName == "Value" || e.PropertyName == "Text")
                {
                    dataGridView1.Rows[cell.rowIndex].Cells[cell.columnIndex].Value = cell.Value;
                }
                else if (e.PropertyName == "BGColor")
                {
                    dataGridView1.Rows[cell.rowIndex].Cells[cell.columnIndex].Style.BackColor = Color.FromArgb(cell.BGColor);
                }
            }
        }

        //OBJECTIVE: SET GUI CELL's TEXT to DATA's TEXT
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //get ref to gui cell
            DataGridViewCell uiCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

            //display data cell's text inside of the gui cell
            Cell cell = sheet.getCell(e.RowIndex, e.ColumnIndex);

            if (null != cell)
            {
                uiCell.Value = sheet.getCell(e.RowIndex, e.ColumnIndex).Text;
            }
        }

        //OBJECTIVE: set the data cell's text equal to the gui cell's text
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            sheet.UnRedoSystem.clearRedo();

            DataGridViewCell uiCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            Cell cell = sheet.getCell(e.RowIndex, e.ColumnIndex);

            if (null != cell)
            {
                string action = "Text Change";
                string old_text = "", new_text = "";

                List<IUndoRedo> commands = new List<IUndoRedo>();
                List<IUndoRedo> current = new List<IUndoRedo>();

                //prevent crash when typing in a cell then erasing it all leaving the cell value as null : can't to string on a null
                if (uiCell.Value != null)
                {
                    current.Add(new RestoreText(uiCell.Value.ToString(), cell.rowIndex, cell.columnIndex));
                    new_text = uiCell.Value.ToString();
                }
                else
                {
                    current.Add(new RestoreText("", cell.rowIndex, cell.columnIndex));
                    new_text = "";
                }

                
                //set data cell's text to the gui cell's text
                //the neccessary events will be triggered to calculate and display the data cell's value
                if (uiCell.Value != null)
                {
                    old_text = cell.Text;
                    commands.Add(new RestoreText(cell.Text, cell.rowIndex, cell.columnIndex));
                    sheet.getCell(e.RowIndex, e.ColumnIndex).Text = uiCell.Value.ToString();
                }
                else//null value won't be converted to "" so we just manually do that
                {
                    old_text = "";
                    new_text = "";
                    commands.Add(new RestoreText(cell.Text, cell.rowIndex, cell.columnIndex));
                    sheet.getCell(e.RowIndex, e.ColumnIndex).Text = "";
                    //comback here not need an undo here
                }

                //set ui sell back to the value of the data cell value
                uiCell.Value = sheet.getCell(e.RowIndex, e.ColumnIndex).Value;

                //add this change to the undostack

                //this prevents undos and redos from being added, just by clicking inside of a cell
                if (old_text != new_text)
                {
                    sheet.UnRedoSystem.addUndo(action, commands);
                    sheet.UnRedoSystem.addUndo(action, current);
                }

                setButtons();
            }
        }

        private void setButtons()
        {
            //stack is empty so disable the button
            if (sheet.UnRedoSystem.emptyUndo() == true)
            {
                undoToolStripMenuItem.Text = "Undo";
                undoToolStripMenuItem.Enabled = false;
            }
            else
            {
                undoToolStripMenuItem.Text = "Undo " + sheet.UnRedoSystem.getTopUndoName();
                undoToolStripMenuItem.Enabled = true;
            }
            //stack is empty so disable the button
            if (sheet.UnRedoSystem.emptyRedo() == true)
            {
                redoToolStripMenuItem.Text = "Redo";
                redoToolStripMenuItem.Enabled = false;
            }
            else
            {
                redoToolStripMenuItem.Text = "Redo " + sheet.UnRedoSystem.getTopRedoName();
                redoToolStripMenuItem.Enabled = true;
            }
        }

        //I use this nothing button to test things
        private void button1_Click(object sender, EventArgs e)
        {

            //Button b = sender as Button;
            //DataGridViewCell uiCell = dataGridView1.Rows[1].Cells[1];
            //DataGridViewCell uiCell2 = dataGridView1.Rows[2].Cells[1];
            ////menuStrip2.BackColor = Color.Blue;
            ////ColorDialog MyDialog = new ColorDialog();
            //// Keeps the user from selecting a custom color.
            ////MyDialog.AllowFullOpen = true;
            //// Allows the user to get help. (The default is false.)
            //// MyDialog.ShowHelp = true;
            //colorDialog1.AllowFullOpen = false;

            //if (colorDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    menuStrip2.BackColor = colorDialog1.Color;
            //    b.Text = colorDialog1.Color.ToArgb().ToString();

            //    uiCell.Value = colorDialog1.Color;
            //    uiCell2.Value = colorDialog1.Color.ToArgb();
            //    uiCell.Style.BackColor = Color.FromArgb(colorDialog1.Color.ToArgb());
            //    uiCell2.Style.BackColor = Color.FromArgb(colorDialog1.Color.ToArgb());
            //    int i = colorDialog1.Color.ToArgb();
            //    uiCell.Value = i.ToString();
            //    uiCell.Style.BackColor = Color.FromArgb(i);
            //}

        }

        private void changeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sheet.UnRedoSystem.clearRedo();

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                int rgb = colorDialog1.Color.ToArgb();//convert users choice into a decimal value (new color)
                int old_color = 0;
                string action = "Color Change";
                List<IUndoRedo> commands = new List<IUndoRedo>();
                List<IUndoRedo> currents = new List<IUndoRedo>();

                //update all selected cells
                for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
                {

                    int row = dataGridView1.SelectedCells[i].RowIndex;
                    int col = dataGridView1.SelectedCells[i].ColumnIndex;

                    //set logic layer to the color that was chosen from the color dialog box
                    //this set will trigger the necessary events to update the UI layer
                    Cell cell = sheet.getCell(row, col);
                    old_color = cell.BGColor;

                    commands.Add(new RestoreColor(old_color, row, col));
                    currents.Add(new RestoreColor(rgb, row, col));

                    if (null != cell)
                    {
                        sheet.getCell(row, col).BGColor = rgb;
                    }
                    else
                    {
                        //this was an issue before I disabled the user's ability to add cols and rows
                        dataGridView1.SelectedCells[i].Value = "No Logic for this Cell";
                    }
                }

                //this order is important
                sheet.UnRedoSystem.addUndo(action, commands);//add undo to system commands
                sheet.UnRedoSystem.addUndo(action, currents);

                setButtons();
            }
        }

        //when you click undo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sheet.UnRedoSystem.undo(sheet);
            setButtons();
        }

        //when user clicks redo
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sheet.UnRedoSystem.redo(sheet);
            setButtons();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //I don't want to ruin the entire program by deleting this
        }

        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file_name = openFileDialog1.FileName;
                sheet.loadSpreadSheet(file_name);
            }
        }


        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file_name = saveFileDialog1.FileName;
                sheet.saveSpreadSheet(file_name);
            }
        }


    }
}
