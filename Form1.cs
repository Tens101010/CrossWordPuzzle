//Please note: This application is purely for my own education, to run through coding 
//examples by following tutorials, and to just tinker around with coding.  
//I know it’s bad practice to heavily comment code (code smell), but comments in all of my 
//exercises will largely be left intact as this serves me 2 purposes:
//    I want to retain what my original thought process was at the time
//    I want to be able to look back in 1..5..10 years to see how far I’ve come
//    And I enjoy commenting on things, however redundant this may be . . . 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Cross_Word_Puzzle
{
    public partial class Form1 : Form
    {
        Clues _clueWindow = new Clues();
        private List<IdCells> _idc = new List<IdCells>();
        public string PuzzleFile = Application.StartupPath + "\\Puzzles\\puzzle_1.pzl";

        public Form1()
        {
            BuildWordList();
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BuildWordList()
        {
            using (var s = new StreamReader(PuzzleFile))
            {
                // Reads the 1st line of the file
                var line = s.ReadLine();
                // For each line that can be read
                while ((line = s.ReadLine()) != null)
                {
                    // Looks at the strings in the file and splits them according to the |
                    String[] l = line.Split('|');
                    _idc.Add(new IdCells(int.Parse(l[0]), int.Parse(l[1]), l[2], l[3], l[4], l[5]));
                    // Adds these array items to the clue window
                    _clueWindow.clue_table.Rows.Add(l[3], l[2], l[5]);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"Crossword Puzzle Game.  2015", @"By Justin Harrison");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeBoard();

            // Defining how the child window opens in relation to the parent window
            // Reads as: Child form will open in relation to the main forms X position + 1 along the same Y coordinate
            _clueWindow.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
            _clueWindow.StartPosition = FormStartPosition.Manual;
            _clueWindow.Show();
            _clueWindow.clue_table.AutoResizeColumns();
        }

        private void InitializeBoard()
        {
            // Creating the black canvas
            board.BackgroundColor = Color.Black;
            board.DefaultCellStyle.BackColor = Color.Black;

            for (int i = 0; i < 21; i++)
            {
                board.Rows.Add();
            }

            // Set width of the columns
            foreach (DataGridViewColumn c in board.Columns)
            {
                c.Width = board.Width / board.Columns.Count;
            }

            // Set height of the rows
            foreach (DataGridViewRow r in board.Rows)
            {
                r.Height = board.Height / board.Rows.Count;
            }

            // Make all cells read only
            for (int row = 0; row < board.Rows.Count; row++)
            {
                for (int col = 0; col < board.Columns.Count; col++)
                {
                    board[col, row].ReadOnly = true;
                }
            }

            foreach (IdCells i in _idc)
            {
                int startCol = i.X;
                int startRow = i.Y;
                char[] word = i.Word.ToCharArray();

                for (int j = 0; j < word.Length; j++)
                {
                    if (i.Direction.ToLower() == "across")
                    {
                        FormatCell(startRow, startCol + j, word[j].ToString());
                    }
                    if (i.Direction.ToLower() == "down")
                    {
                        FormatCell(startRow + j, startCol, word[j].ToString());
                    }
                }
            }
        }

        private void FormatCell(int row, int col, string letter)
        {
            DataGridViewCell c = board[col, row];
            c.Style.BackColor = Color.White;
            c.ReadOnly = false;
            c.Style.SelectionBackColor = Color.Cyan;
            c.Tag = letter;
        }

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            // This will make the child window snap to the parent window
            _clueWindow.SetDesktopLocation(this.Location.X + this.Width + 1, this.Location.Y);
        }

        private void board_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Make the first letter uppercase 
            try
            {
                board[e.ColumnIndex, e.RowIndex].Value = board[e.ColumnIndex, e.RowIndex].Value.ToString().ToUpper();
            }
            catch { }

            // Truncate to one letter if user enters more than 1 
            try
            {
                if (board[e.ColumnIndex, e.RowIndex].Value.ToString().Length > 1)
                {
                    board[e.ColumnIndex, e.RowIndex].Value =
                        board[e.ColumnIndex, e.RowIndex].Value.ToString().Substring(0, 1);
                }
            }
            catch { }

            // Format color if correct
            try
            {
                board[e.ColumnIndex, e.RowIndex].Style.ForeColor =
                    board[e.ColumnIndex, e.RowIndex].Value.Equals(board[e.ColumnIndex, e.RowIndex].Tag.ToString().ToUpper())
                    ? Color.Blue : Color.Red;
            }
            catch { }
        }

        private void openPuzzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Puzzle Files|*.pzl";
            if (ofd.ShowDialog().Equals(DialogResult.OK))
            {
                PuzzleFile = ofd.FileName;
                board.Rows.Clear();
                _clueWindow.clue_table.Rows.Clear();
                _idc.Clear();
                BuildWordList();
                InitializeBoard();
            }
        }

        private void board_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            string number = "";

            // foreach(item c in List of items)
            if (_idc.Any(c => (number = c.Number) != "" && c.X == e.ColumnIndex && c.Y == e.RowIndex))
            {
                var r = new Rectangle(e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width, e.CellBounds.Height);
                e.Graphics.FillRectangle(Brushes.White, r);
                var f = new Font(e.CellStyle.Font.FontFamily, 7);
                e.Graphics.DrawString(number, f, Brushes.Black, r);
                e.PaintContent(e.ClipBounds);
                e.Handled = true;
            }
        }
    }// End class Form1 : Form

    public class IdCells
    {
        public int X;
        public int Y;
        public String Direction;
        public string Number;
        public String Word;
        public string Clue;

        public IdCells(int x, int y, string d, string n, string w, string c)
        {
            this.X = x;
            this.Y = y;
            this.Direction = d;
            this.Number = n;
            this.Word = w;
            this.Clue = c;
        }
    }// End IdCells class

}
