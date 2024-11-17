using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASLLearningApp
{
    public partial class LearningMenu : UserControl
    {
        private RichTextBox tbQuestionBox;
        //private Quiz quiz;
        private ComboBox cbSelectLetter;
        private Webcam webcam;
        private TextBox lbDebug;
        private PictureBox pbInstruction;
        private RichTextBox rtbTestInstructions;
        private char currentCharacter = 'n';
        private Button btBegin;

        // Delegate for the event for when the symbol is changed
        public delegate void ExitUCHandler(object sender, EventArgs e);

        // Event
        public event ExitUCHandler UCExit;

        public LearningMenu()
        {
            InitializeComponent();
        }

        private void LearningMenu_Load(object sender, EventArgs e)
        {
            Setup();
        }

        private void Setup()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            List<char> chars = new List<char>();
            chars.AddRange(new char[] { 'A', 'B', 'C', 'D', 'E', 'S', 'T' });
            this.Size = this.ParentForm.Size;
            int xConstant = this.Size.Width/2;
            int yConstant = this.Size.Height/2;
            int padding = 5;

            // Create the text box that contains the current word as a question.
            tbQuestionBox = new RichTextBox()
            {
                Size = new Size(xConstant/4, xConstant/4),
                Enabled = false,
                Font = new Font("Arial", 54, FontStyle.Bold),
                Text = "\n" + 'A',

            };
            tbQuestionBox.Location = new Point(xConstant - (tbQuestionBox.Width), 100);
            tbQuestionBox.BringToFront();
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;

            // Create the picture box that contains the instruction for signing a character.
            pbInstruction = new PictureBox()
            {
                Size = tbQuestionBox.Size,
                Location = new Point(tbQuestionBox.Location.X + tbQuestionBox.Width, tbQuestionBox.Location.Y),
                BorderStyle = BorderStyle.FixedSingle
            };
            string imagePath = "asl.jpg";
            pbInstruction.Image = Image.FromFile(imagePath);
            pbInstruction.SizeMode = PictureBoxSizeMode.StretchImage;


            // ComboBox that allows the user to swap between any question quickly.
            cbSelectLetter = new ComboBox();
            cbSelectLetter.Location = new Point(xConstant + xConstant / 2, tbQuestionBox.Location.Y);
            cbSelectLetter.Size = new Size(yConstant/10, xConstant / 8);
            for (int i = 0; i < chars.Count; i++)
                cbSelectLetter.Items.Add(chars[i]);
            cbSelectLetter.SelectedIndex = 0;
            cbSelectLetter.BringToFront();
            cbSelectLetter.SelectedIndexChanged += new EventHandler(cbSelectLetter_SelectedIndexChanged);

            // Create Webcam
            webcam = new Webcam();
            webcam.SignedSymbolChanged += Webcam_GetCurrentSignedSymbol;
            webcam.Location = new Point(cbSelectLetter.Location.X - webcam.Width + cbSelectLetter.Width, 400);
            webcam.BringToFront();
            this.Controls.Add(webcam);
            

            // Create the button to traverse to the next word.
            Button btNext = new Button()
            {
                Text = "Next",
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Size = new Size(xConstant/6, yConstant/10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btNext.Location = new Point(xConstant + 5, xConstant / 2);
            btNext.BringToFront();
            btNext.Click += new EventHandler(btNext_Click);

            // Create the button to traverse to the previous word.
            Button btPrev = new Button() {
                Location = new Point(btNext.Location.X-btNext.Width-(padding*2), btNext.Location.Y),
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Previous",
                Size = btNext.Size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btPrev.Click += new EventHandler(btPrev_Click);
            btPrev.BringToFront();

            // Create the button to exit the learn menu.
            Button btExit = new Button()
            {
                Size = new Size((this.Size.Width / 2) / 6, 75),
                Enabled = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Exit",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            btExit.Location = new Point(0, this.Height - (int)(btExit.Height * 1.5));
            btExit.Click += new EventHandler(btExit_Click);

            // Create the button to start attempting to sign the current letter.
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

            // Add all controls to the UserControl itself.
            this.Controls.Add(btNext);
            this.Controls.Add(btPrev);
            this.Controls.Add(tbQuestionBox);
            this.Controls.Add(cbSelectLetter);
            this.Controls.Add(pbInstruction);
            this.Controls.Add(btExit);
            this.Controls.Add(btBegin);

            // Show test
            rtbTestInstructions = new RichTextBox()
            {
                Size = new Size(tbQuestionBox.Width*2, tbQuestionBox.Height / 2),
                Location = new Point(tbQuestionBox.Location.X, tbQuestionBox.Location.Y + tbQuestionBox.Height + 10),
                Font = new Font("Arial", 18, FontStyle.Bold)
            };
            rtbTestInstructions.SelectionAlignment = HorizontalAlignment.Center;
            rtbTestInstructions.Visible = true;
            rtbTestInstructions.Enabled = false;
            this.Controls.Add(rtbTestInstructions);

            // DEBUG TEXT BOX
            lbDebug = new TextBox();
            lbDebug.Location = new Point(webcam.Location.X + (webcam.Width / 2 - lbDebug.Width / 2), webcam.Location.Y + 300);
            lbDebug.BringToFront();
            lbDebug.Text = "No symbol yet...";
            this.Controls.Add(lbDebug);
            lbDebug.Visible = false;
            webcam.Visible = false;
        }

        // Advance to the next letter.
        private void btNext_Click(object sender, EventArgs e)
        {
            if (cbSelectLetter.SelectedIndex != cbSelectLetter.Items.Count - 1) {
                cbSelectLetter.SelectedIndex = cbSelectLetter.SelectedIndex + 1;
                tbQuestionBox.Text = "\n" + cbSelectLetter.Items[cbSelectLetter.SelectedIndex];
                tbQuestionBox.SelectAll();
                tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            }
        }

        // Go back to the previous letter.
        private void btPrev_Click(object sender, EventArgs e)
        {
            if ((cbSelectLetter.SelectedIndex != 0))
            {
                cbSelectLetter.SelectedIndex = cbSelectLetter.SelectedIndex - 1;
                tbQuestionBox.Text = "\n" + cbSelectLetter.Items[cbSelectLetter.SelectedIndex];
                tbQuestionBox.SelectAll();
                tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            }
        }

        // Begin testing the user's ability to finger spell the letter.
        private void btBegin_Click(object sender, EventArgs e)
        {
            (sender as Button).Enabled = false;
            Thread doTest = new Thread(TryLetter);
            doTest.Start();
        }

        // Jump to a letter.
        private void cbSelectLetter_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbQuestionBox.Text = "\n" + cbSelectLetter.Items[(sender as ComboBox).SelectedIndex];
            tbQuestionBox.SelectAll();
            tbQuestionBox.SelectionAlignment = HorizontalAlignment.Center;
            btBegin.Enabled = true;
        }

        // Get the currently signed letter by the user.
        private void Webcam_GetCurrentSignedSymbol(object sender, string symbol)
        {
            lbDebug.Text = "" + symbol;
            currentCharacter = ("" + symbol + "n")[0];
        }

        // Exit the learn menu and return to the main menu.
        private void btExit_Click(object sender, EventArgs e)
        {
            webcam.Exit();
            UCExit?.Invoke(this, e);
        }

        // Let the user try to finger spell the current letter.
        private async void TryLetter()
        {
            // Make the instructions visible to the user.
            rtbTestInstructions.Visible = true;
            rtbTestInstructions.BringToFront();
            rtbTestInstructions.Text = "\nGet ready...";
            // Give the user two seconds to prepare.
            await Task.Delay(2000);
            for (int i = 1; i < tbQuestionBox.Text.Length; i++)
            {
                char character = tbQuestionBox.Text.ToUpper()[i];
                if ((character == ' ') || (character + "" == "\n"))
                {
                    continue;
                }
                tbQuestionBox.Select(i, 1);
                tbQuestionBox.SelectionFont = new Font("Times New Roma", 54, FontStyle.Underline);
                tbQuestionBox.SelectionBackColor = Color.Yellow;
                // Wait three seconds before seeing if the user spelled the letter correctly.
                for (int k = 3; k > -1; k--)
                {
                    // Let the user know how many seconds are left to hold up the letter.
                    rtbTestInstructions.Text = "\nSpell " + character + " in..." + k;
                    await Task.Delay(1000);
                }

                tbQuestionBox.SelectionFont = new Font("Times New Roma", 54, FontStyle.Regular);
                tbQuestionBox.SelectionBackColor = Color.White;
                // Congratulate the user on successfully signing the character, else let them know they got it wrong.
                if (currentCharacter == character)
                {
                    rtbTestInstructions.Text = "\nYou got it right!";
                    await Task.Delay(2000);
                    rtbTestInstructions.Text = "";
                }
                else
                {
                    rtbTestInstructions.Text = "\nYou got that one wrong!";
                    await Task.Delay(2000);
                    rtbTestInstructions.Text = "";
                    break;
                }
            }
            btBegin.Enabled = true;
        }
    }
}
