using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASLLearningApp
{
    public partial class QuizSelectionMenu : UserControl
    {
        private List<Control> quizLandingCurrentControls = new List<Control>();
        private QuizGradeHoverDisplay easyDisplayedGrades = new QuizGradeHoverDisplay("Easy");
        private QuizGradeHoverDisplay intermediateDisplayedGrades = new QuizGradeHoverDisplay("Intermediate");
        private QuizGradeHoverDisplay hardDisplayedGrades = new QuizGradeHoverDisplay("Hard");
        private QuizGradeHoverDisplay currentDisplayedGrades;
        private QuizMenu currentQuiz;

        // Delegate for the event for when the quiz slection menu is closed
        public delegate void ExitUCHandler(object sender, EventArgs e);

        // Event
        public event ExitUCHandler UCExit;

        public QuizSelectionMenu()
        {
            InitializeComponent();
        }

        // Method run when the quiz selection menu is created
        private void QuizSelectionMenu_Load(object sender, EventArgs e)
        {
            Setup();
        }

        // Methd to setup the quiz selection menu
        private void Setup()
        {
            this.Size = this.ParentForm.Size;
            // Initialize the three buttons to select a quiz difficulty
            Button btEasy = new Button();
            btEasy.Text = "Easy";
            Button btIntermediate = new Button();
            btIntermediate.Text = "Intermediate";
            Button btHard = new Button();
            btHard.Text = "Hard";
            List<Button> btList = new List<Button>() { btEasy, btIntermediate, btHard };
            int yConstant = this.Size.Width / 12;
            int posY = 0;
            // Setup the exit button
            Button btExit = new Button()
            {
                Size = new Size((this.Size.Width /2) / 6, 75),
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Exit",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btExit.Location = new Point(0, this.Height - (int)(btExit.Height * 1.5));
            btExit.Click += new EventHandler(btExit_Click);
            this.Controls.Add(btExit);
            // Setup the difficulty selection buttons
            foreach (Button bt in btList)
            {
                posY += yConstant;
                bt.Location = new Point(this.Size.Width / 2 - 375 - 50 - 2, posY);
                bt.Size = new Size(700, 75);
                bt.Font = new Font("Arial", 24, FontStyle.Bold);
                bt.Enabled = true;
                bt.BringToFront();
                bt.Click += new EventHandler(btSelection_Click);
                bt.FlatStyle = FlatStyle.Flat;
                bt.BackColor = Color.White;
                bt.ForeColor = Color.Black;
                this.Controls.Add(bt);
                quizLandingCurrentControls.Add(bt);
            }
            posY = 0;
            // Setup buttons to the side of each difficulty to let users view their quiz scores and reset their quizzes
            for (int i = 0; i < 3; i++)
            {
                Button btGrade = new Button();
                posY += yConstant;
                btGrade.Location = new Point(this.Size.Width / 2 + 275 + 2, posY);
                btGrade.Size = new Size(75, 75);
                btGrade.Font = new Font("Arial", 8, FontStyle.Bold);
                btGrade.Text = "_____\n_____\n_____";
                // Giive each button a tag with their associated difficulty in order to get the quiz grades
                if (i == 0)
                    btGrade.Tag = "Easy";
                else if (i == 1)
                    btGrade.Tag = "Intermediate";
                else if (i == 2)
                    btGrade.Tag = "Hard";

                btGrade.BackColor = Color.White;
                btGrade.BringToFront();
                btGrade.FlatStyle = FlatStyle.Flat;
                btGrade.MouseEnter += new EventHandler(grade_MouseEnter);
                btGrade.MouseLeave += new EventHandler(grade_MouseLeave);
                btGrade.Click += new EventHandler(grade_Click);

                quizLandingCurrentControls.Add(btGrade);
                this.Controls.Add(btGrade);
            }
            quizLandingCurrentControls.Add(btExit);
        }

        // Handler for clicking a quiz difficulty
        private void btSelection_Click(object sender, EventArgs e)
        {
            currentQuiz = new QuizMenu((sender as Button).Text);
            currentQuiz.Location = new Point(200, 200);
            currentQuiz.Dock = DockStyle.Fill;
            currentQuiz.BringToFront();
            foreach (Control bt in quizLandingCurrentControls)
                bt.Hide();
            this.Controls.Add(currentQuiz);
            currentQuiz.UCExit += quiz_exited;
            currentQuiz.Show();
        }

        // Handler for clicking to clear all grades from a difficulty
        private void grade_Click(object sender, EventArgs e)
        {
            // string diff = (sender as Button).Tag.ToString();
            currentDisplayedGrades.ClearFile();
        }

        // Handler to begin displaying the quiz grades in a hovering display that will follow the mouse
        private void grade_MouseEnter(object sender, EventArgs e)
        {
            // QuizGradeHoverDisplay currentDisplayedGrades;
            string diff = (sender as Button).Tag.ToString();
            // Get the current grade based on difficulty
            if (diff == "Easy")
                currentDisplayedGrades = easyDisplayedGrades;
            else if (diff == "Intermediate")
                currentDisplayedGrades = intermediateDisplayedGrades;
            else
                currentDisplayedGrades = hardDisplayedGrades;
            currentDisplayedGrades.Location = MousePosition;
            currentDisplayedGrades.BringToFront();
            if (!this.Controls.Contains(currentDisplayedGrades))
                this.Controls.Add(currentDisplayedGrades);
            (sender as Button).MouseMove += grade_MouseMove;
            currentDisplayedGrades.Show();
        }

        // Handler to stop displaying the quiz grades when the mouse leaves the button
        private void grade_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).MouseMove -= grade_MouseMove;
            currentDisplayedGrades.Hide();
        }

        // Handler to move the grade display with the mouse
        private void grade_MouseMove(object sender, MouseEventArgs e)
        {
            currentDisplayedGrades.Location = MousePosition;
        }

        // Method to shut down a selected quiz and recieve its score to save it
        private void quiz_exited(object sender, EventArgs e, double grade, string difficulty)
        {
            currentQuiz.Dispose();
            // Show all the previously hidden controls.
            foreach (Control bt in quizLandingCurrentControls)
                bt.Show();
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DateTime currentDateTime = DateTime.Now;
            // Save the score to the correct location.
            using (StreamWriter stream = File.AppendText(currentDirectory+"\\Grades\\"+difficulty+".txt"))
            {
                stream.WriteLine("Taken at "+currentDateTime.ToString()+" with a grade of "+grade+"%");
            }
            // Refresh all the possible grades.
            easyDisplayedGrades.RefreshGrades();
            intermediateDisplayedGrades.RefreshGrades();
            hardDisplayedGrades.RefreshGrades();
        }

        // Exit this menu to return to the main menu.
        private void btExit_Click(object sender, EventArgs e)
        {
            UCExit?.Invoke(this, e);
        }
    }
}
