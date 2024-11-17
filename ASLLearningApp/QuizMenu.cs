using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ASLLearningApp
{
    public partial class QuizMenu : UserControl
    {
        private RichTextBox tbQuestionBox = new RichTextBox();
        private Quiz quiz;
        private ComboBox cbSelectQuestion;
        private Webcam webcam;
        private TextBox tbDebug = new TextBox();
        // private RichTextBox rtbStrikes;
        private char currentCharacter = 'n';
        private Button btBegin;
        private RichTextBox rtbTestInstructions;
        private List<RichTextBox> rtbStrikesCollection = new List<RichTextBox>(3);
        private int questionNo = 0;
        private string givenDifficulty;

        // Delegate for the event for when the symbol is changed
        public delegate void ExitUCHandler(object sender, EventArgs e, double grade, string difficulty);

        // Event
        public event ExitUCHandler UCExit;

        public QuizMenu(string difficulty)
        {
            givenDifficulty = difficulty;
            InitializeComponent();
        }

        private void Quiz_Load(object sender, EventArgs e)
        {
            Setup();
        }

        private void Setup()
        {
            // Disable checking for illegal cross thread calls on controls so another thread can change properties of the controls
            Control.CheckForIllegalCrossThreadCalls = false;
            quiz = new Quiz(givenDifficulty);
            this.Size = this.ParentForm.Size;
            int xConstant = this.Size.Width / 2;
            int yConstant = this.Size.Height / 10;
            // Create the button to traverse to the next word.
            Button btNext = new Button()
            {
                Size = new Size(xConstant / 6, 75),
                Location = new Point(xConstant + 5, xConstant / 2),
                Text = "Next",
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btNext.BringToFront();
            btNext.Click += new EventHandler(btNext_Click);
            // Create the button to traverse to the previous word.
            Button btPrev = new Button()
            {
                Size = btNext.Size,
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Previous",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btPrev.Location = new Point((xConstant - btPrev.Width) - 5, xConstant / 2);
            btPrev.BringToFront();
            btPrev.Click += new EventHandler(btPrev_Click);
            // Create the button to be ready to spell the word
            btBegin = new Button()
            {
                Size = btNext.Size,
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Answer",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btBegin.Location = new Point(btNext.Location.X - btBegin.Width / 2 - 5, btNext.Location.Y + btBegin.Height);
            btBegin.Click += new EventHandler(btBegin_Click);
            // Create a button to exit
            Button btExit = new Button()
            {
                Size = btNext.Size,
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Exit",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btExit.Location = new Point(0, this.Height - (int)(btExit.Height*1.5));
            btExit.Click += new EventHandler(btExit_Click);
            // Create the text box that contains the current word as a question.
            tbQuestionBox = new RichTextBox();
            tbQuestionBox.Size = new Size(xConstant / 2, xConstant / 4);
            tbQuestionBox.Location = new Point(xConstant - tbQuestionBox.Width / 2, yConstant / 2);
            tbQuestionBox.Enabled = false;
            tbQuestionBox.BringToFront();
            tbQuestionBox.Font = new Font("Arial", 54, FontStyle.Bold);
            tbQuestionBox.Text = "\n" + quiz.CurrentWord();
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            // ComboBox that allows the user to swap between any question quickly.
            cbSelectQuestion = new ComboBox();
            cbSelectQuestion.Location = new Point(xConstant + xConstant / 2, tbQuestionBox.Location.Y);
            cbSelectQuestion.Size = new Size(40, 40);
            for (int i = 0; i < quiz.Size(); i++)
                cbSelectQuestion.Items.Add(i + 1 + "");
            cbSelectQuestion.SelectedIndex = 0;
            cbSelectQuestion.BringToFront();
            cbSelectQuestion.SelectedIndexChanged += new EventHandler(cbSelectQuestion_SelectedIndexChanged);
            // Rich text boxes to show the number of strikes the user has for a word.
            RichTextBox rtb1 = new RichTextBox();
            RichTextBox rtb2 = new RichTextBox();
            RichTextBox rtb3 = new RichTextBox();
            rtbStrikesCollection.Add(rtb1);
            rtbStrikesCollection.Add(rtb2);
            rtbStrikesCollection.Add(rtb3);
            int currentXOffset = 0;
            for (int i = 0; i < 3; i++)
            {
                rtbStrikesCollection[i].Size = new Size(60, 60);
                rtbStrikesCollection[i].Font = new Font("Arial", 28, FontStyle.Bold);
                rtbStrikesCollection[i].Text = "X";
                rtbStrikesCollection[i].Enabled = false;
                rtbStrikesCollection[i].Location = new Point(tbQuestionBox.Location.X - 65, tbQuestionBox.Location.Y+currentXOffset);
                rtbStrikesCollection[i].SelectionAlignment = HorizontalAlignment.Center;
                this.Controls.Add(rtbStrikesCollection[i]);
                currentXOffset += 60;
            }
            rtbTestInstructions = new RichTextBox()
            {
                Size = new Size(tbQuestionBox.Width, tbQuestionBox.Height/2),
                Location = new Point(tbQuestionBox.Location.X, tbQuestionBox.Location.Y+tbQuestionBox.Height+10),
                Font = new Font("Arial", 18, FontStyle.Bold)
            };
            rtbTestInstructions.SelectionAlignment = HorizontalAlignment.Center;
            rtbTestInstructions.Visible = true;
            rtbTestInstructions.Enabled = false;
            // Add all controls to the UserControl to display them
            this.Controls.Add(btNext);
            this.Controls.Add(btPrev);
            this.Controls.Add(tbQuestionBox);
            this.Controls.Add(cbSelectQuestion);
            this.Controls.Add(btBegin);
            this.Controls.Add(btExit);
            // this.Controls.Add(rtbStrikes);
            this.Controls.Add(rtbTestInstructions);
            // Create Webcam
            webcam = new Webcam();
            webcam.SignedSymbolChanged += Webcam_GetCurrentSignedSymbol;
            // The following will align the webcam's border up with the edge of the combobox for selecting questions
            webcam.Location = new Point(cbSelectQuestion.Location.X - webcam.Width + cbSelectQuestion.Width, 400);
            webcam.BringToFront();
            this.Controls.Add(webcam);
            // DEBUG TEXT BOX
            tbDebug = new TextBox();
            tbDebug.Location = new Point(webcam.Location.X + (webcam.Width / 2 - tbDebug.Width / 2), webcam.Location.Y + 300);
            tbDebug.BringToFront();
            tbDebug.Text = "No symbol yet...";
            tbDebug.Visible = false;
            this.Controls.Add(tbDebug);
            SetStrikes();
            webcam.Visible = false;
        }

        // Advance to the next question.
        private void btNext_Click(object sender, EventArgs e)
        {
            tbQuestionBox.Text = "\n" + quiz.NextWord();
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            if (!(cbSelectQuestion.SelectedIndex == cbSelectQuestion.Items.Count - 1))
            {
                cbSelectQuestion.SelectedIndex = cbSelectQuestion.SelectedIndex + 1;
            }
        }

        // Go back to the previous question.
        private void btPrev_Click(object sender, EventArgs e)
        {
            tbQuestionBox.Text = "\n" + quiz.PrevWord();
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            if ((cbSelectQuestion.SelectedIndex != 0))
            {
                cbSelectQuestion.SelectedIndex = cbSelectQuestion.SelectedIndex - 1;
            }
        }

        // Jump to a selected question.
        private void cbSelectQuestion_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbQuestionBox.Text = "\n" + quiz.JumpToWord((sender as ComboBox).SelectedIndex);
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            questionNo = cbSelectQuestion.SelectedIndex;
            SetStrikes();
        }

        // Get the currently signed letter and hold the letter in storage until it is used.
        private void Webcam_GetCurrentSignedSymbol(object sender, string symbol)
        {
            tbDebug.Text = "" + symbol;
            if (tbDebug.Text.Length > 0)
                currentCharacter = tbDebug.Text[0];
        }

        // Button to start being tested in signing the word
        private void btBegin_Click(object sender, EventArgs e)
        {
            (sender as Button).Enabled = false;
            Thread doTest = new Thread(DoTest);
            doTest.Start();
        }

        // Test the user on if they are correctly doing fingerspelling.
        private async void DoTest()
        {
            // Make the instructions visible to the user.
            rtbTestInstructions.Visible = true;
            rtbTestInstructions.BringToFront();
            rtbTestInstructions.Text = "\nGet ready...";
            // Give the user a few seconds to prepare before starting after clicking the button.
            await Task.Delay(5000);
            // Traverse through the word for each letter they need to finger spell.
            for (int i = 1; i < tbQuestionBox.Text.Length; i++)
            {
                // Make the letter uppercase since all letters from the Python script come as uppercase.
                char character = tbQuestionBox.Text.ToUpper()[i];
                if ((character == ' ') || (character+"" == "\n"))
                {
                    continue;
                }
                tbQuestionBox.Select(i, 1);
                tbQuestionBox.SelectionFont = new Font("Times New Roma", 54, FontStyle.Underline);
                tbQuestionBox.SelectionBackColor = Color.Yellow;
                // Give the user three seconds to finger spell the letter.
                for (int k = 3; k > -1; k--)
                {
                    // Tell the user their remaining time
                    rtbTestInstructions.Text = "\nSpell "+character+" in..."+k;
                    await Task.Delay(1000);
                }
                
                tbQuestionBox.SelectionFont = new Font("Times New Roma", 54, FontStyle.Regular);
                tbQuestionBox.SelectionBackColor = Color.White;
                // If the user signed the correct letter congratulate them, else let them know they did it wrong.
                if (currentCharacter == character)
                {
                    rtbTestInstructions.Text = "\nYou got it right!";
                    await Task.Delay(2000);
                    rtbTestInstructions.Text = "";
                    // Save that the word was spelled correctly.
                    if (i == tbQuestionBox.Text.Length-1)
                        quiz.SuccessfulSpelling(questionNo);
                }
                else
                {
                    rtbTestInstructions.Text = "\nYou got that one wrong!";
                    // Add a strike and let all the current strikes be visible.
                    AddStrike();
                    SetStrikes();
                    await Task.Delay(2000);
                    rtbTestInstructions.Text = "";
                    break;
                }
            }
            // If the user does not have more than three strikes, let them redo the question.
            if (quiz.GetStrikes(questionNo) != 3 || !quiz.CheckSuccessful(questionNo))
                btBegin.Enabled = true;
        }

        // Add a strike against the user's spelling.
        private void AddStrike()
        {
            if (givenDifficulty != "Easy")
                quiz.AddStrike(questionNo);
        }

        // Show all the strikes the user currently has.
        private void SetStrikes()
        {
            for (int i = 0; i < 3; i++)
            {
                RichTextBox current = rtbStrikesCollection[i];
                current.Select(0, 1);
                current.SelectionColor = Color.Black;
                current.Select(0, 0);
            }
            int strikes = quiz.GetStrikes(questionNo);
            btBegin.Enabled = true;
            if (strikes == 3){
                btBegin.Enabled = false;
            }
            for (int i = 0; i < strikes; i++)
            {
                RichTextBox current = rtbStrikesCollection[i];
                current.Select(0, 1);
                current.SelectionColor = Color.Red;
                current.Select(0, 0);
            }
        }

        // Exit the quiz and return to the quiz selection menu.
        private void btExit_Click(object sender, EventArgs e)
        {
            webcam.Exit();
            double grade = quiz.GetGrade();
            UCExit?.Invoke(this, e, grade, givenDifficulty);
        }
    }
}
