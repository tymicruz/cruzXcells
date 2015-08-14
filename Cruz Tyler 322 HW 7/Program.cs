/*
 * Name: Tyler Cruz
 * ID:  11333476
 * 
 * Description: 
 * 
 * HW9
 * Implement loading an xml file
 * Implement saving spreadsheet to an xml sheet
 * 
 * Progress towards an app similar to excel
 * Enter text into gui cell and the text will be process by the Spreadsheet Engine
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cruz_Tyler_322_HW_10
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
