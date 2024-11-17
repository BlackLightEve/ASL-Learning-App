using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASLLearningApp
{
    public partial class QuizGradeHoverDisplay : UserControl
    {
        private RichTextBox rtbGradeDisplay = new RichTextBox();
        private string difficulty = "";

        // Constructor for the quiz grade hover display
        public QuizGradeHoverDisplay(string difficulty)
        {
            InitializeComponent();
            this.difficulty = difficulty;
        }

        private void QuizGradeHoverDisplay_Load(object sender, EventArgs e)
        {
            Setup();
        }

        // Setup the the user control
        private void Setup()
        {
            // Get the current directory to retrieve the grades
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.BackColor = Color.White;
            rtbGradeDisplay.Size = this.Size;
            // Get all the saved grades base on the difficulty
            rtbGradeDisplay.Text = File.ReadAllText(currentDirectory + "\\grades\\" + difficulty + ".txt");
            this.Controls.Add(rtbGradeDisplay);
        }

        // Clear all the grades saved to the file corresponding to the difficulty
        public void ClearFile()
        {
            // Get ask the user if they're sure to clear the file
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear your recorded grades?", "WARNING", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = currentDirectory + "\\grades\\" + difficulty + ".txt";
                // Clear the file entirely
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write("");
                }
                // Clear the text
                rtbGradeDisplay.Text = File.ReadAllText(filePath);
            }
            // If the user chooses not to clear the file then do nothing
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
            
        }

        // Refresh the text box to show the current contents of the file corresponding to difficulty
        public void RefreshGrades()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = currentDirectory + "\\grades\\" + difficulty + ".txt";
            rtbGradeDisplay.Text = File.ReadAllText(filePath);
        }
    }
}
