using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Word = Microsoft.Office.Interop.Word;

namespace ASLLearningApp
{
    public partial class FreeWriteMenu : UserControl
    {
        private RichTextBox rtbTextField;
        private RichTextBox rtbNextChar;
        private char nextChar = 'n';
        private ProgressBar pbrNextCharTimer;
        private Button btBold = new Button();
        private Button btItalics = new Button();
        private Button btUnderline = new Button();
        private Button btSave = new Button();
        private Button btSaveAs = new Button();
        private Button btOpen = new Button();
        private ComboBox cbColors = new ComboBox();
        private Webcam webcam;
        private bool writable = true;
        private string currentFile = "";
        private static readonly SemaphoreSlim lockGestureDelay_Semaphore = new SemaphoreSlim(1, 1);

        // Delegate for the event for when the symbol is changed
        public delegate void ExitUCHandler(object sender, EventArgs e);

        // Event
        public event ExitUCHandler UCExit;

        public FreeWriteMenu()
        {
            InitializeComponent();
        }

        private void FreeWriteMenu_Load(object sender, EventArgs e)
        {
            Setup();
        }

        private void Setup()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Size = this.ParentForm.Size;
            int yConstant = this.Size.Width / 2;
            int posX = -32;
            // Create text field
            rtbTextField = new RichTextBox();
            rtbTextField.Size = new Size(this.Size.Width / 2, (int)(this.Size.Height / 1.2));
            rtbTextField.Location = new Point((this.Size.Width/2) - rtbTextField.Width/2, 110);
            rtbTextField.Font = new Font("Times New Roman", 24, FontStyle.Regular);
            this.Controls.Add(rtbTextField);

            //btSave, btSaveAs, and btOpen creation
            btSave.Text = "Save";
            btSave.Font = new Font("Courier New", 20, FontStyle.Bold);
            btSave.Size = new Size(200, 35);
            btSave.BackColor = Color.White;
            btSave.Location = new Point(rtbTextField.Location.X, 0);
            btSave.Click += new EventHandler(btSave_Click);
            btSave.Enabled = false;
            this.Controls.Add(btSave);
            btSaveAs.Text = "Save As";
            btSaveAs.Font = btSave.Font;
            btSaveAs.BackColor = btSave.BackColor;
            btSaveAs.Location = new Point(btSave.Location.X, btSave.Location.Y + btSave.Height);
            btSaveAs.Size = btSave.Size;
            btSaveAs.Click += new EventHandler(btSaveAs_Click);
            this.Controls.Add(btSaveAs);
            btOpen.Text = "Open";
            btOpen.Font = btSave.Font;
            btOpen.BackColor = btSave.BackColor;
            btOpen.Location = new Point(btSave.Location.X, btSaveAs.Location.Y + btSaveAs.Height);
            btOpen.Size = btSave.Size;
            btOpen.Click += new EventHandler(btOpen_Click);
            this.Controls.Add(btOpen);

            // Create the Toolbar Buttons.
            List<Button> toolBar = new List<Button>() { btBold, btItalics, btUnderline, };
            // Setup all buttons to modify text properties
            posX = btSave.Location.X+btSave.Width;
            Label lbFontStyles = new Label();
            lbFontStyles.Text = "Font";
            lbFontStyles.Location = new Point(posX+42, 0);
            lbFontStyles.Size = new Size(120, 35);
            lbFontStyles.BackColor = Color.LightSteelBlue;
            lbFontStyles.Font = new Font("Courier New", 20, FontStyle.Bold);
            lbFontStyles.TextAlign = ContentAlignment.MiddleCenter;
            lbFontStyles.ForeColor = Color.White;
            this.Controls.Add(lbFontStyles);
            foreach (Button bt in toolBar)
            {
                posX += 42;
                bt.Location = new Point(posX, lbFontStyles.Location.Y+35);
                bt.Size = new Size(35, 35);
                bt.TextAlign = ContentAlignment.MiddleCenter;
                bt.BackColor = Color.White;
                bt.Click += new EventHandler(btTextModifier_Click);
                this.Controls.Add(bt);
            }
            btBold.Text = "B";
            btBold.Font = new Font("Courier New", 16, FontStyle.Bold);
            btItalics.Text = "I";
            btItalics.Font = new Font("Courier New", 16, FontStyle.Italic);
            btUnderline.Text = "U";
            btUnderline.Font = new Font("Times New Roman", 16, FontStyle.Underline);

            // Create the button to exit this menu.
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
            this.Controls.Add(btExit);

            // Create colors drop down.
            Label lbColors = new Label();
            lbColors.Text = "Color";
            lbColors.Location = new Point(btUnderline.Location.X + 42 + btUnderline.Width, 0);
            lbColors.Size = new Size(120, 35);
            lbColors.BackColor = Color.LightSteelBlue;
            lbColors.Font = new Font("Courier New", 20, FontStyle.Bold);
            lbColors.TextAlign = ContentAlignment.MiddleCenter;
            lbColors.ForeColor = Color.White;
            cbColors.Location = new Point(lbColors.Location.X, lbColors.Height);
            cbColors.Font = new Font("Courier New", 16);
            cbColors.Items.Add("Black");
            cbColors.Items.Add("Red");
            cbColors.Items.Add("Blue");
            cbColors.Items.Add("Green");
            cbColors.SelectedIndex = 0;
            cbColors.SelectionChangeCommitted += cbColors_SelectionChangeCommitted;
            this.Controls.Add(cbColors);
            this.Controls.Add(lbColors);

            // Toolbar
            PictureBox pbToolbar = new PictureBox();
            pbToolbar.Dock = DockStyle.Top;
            pbToolbar.BackColor = Color.LightSteelBlue;
            pbToolbar.Height = 100;
            this.Controls.Add(pbToolbar);

            // Create Webcam
            webcam = new Webcam();
            webcam.SignedSymbolChanged += Webcam_GetCurrentSignedSymbol;
            webcam.Location = new Point(rtbTextField.Location.X+rtbTextField.Width, rtbTextField.Location.Y + rtbTextField.Height / 2 - webcam.Height / 2);
            webcam.BringToFront();
            this.Controls.Add(webcam);

            // Create next word box
            rtbNextChar = new RichTextBox();
            rtbNextChar.Location = webcam.Location;
            rtbNextChar.Size = webcam.Size;
            rtbNextChar.BringToFront();
            rtbNextChar.Enabled = false;
            rtbNextChar.Font = new Font("Courier New", 64, FontStyle.Bold);
            rtbNextChar.SelectionAlignment = HorizontalAlignment.Center;
            this.Controls.Add(rtbNextChar);

            // Create the progress bar for the next word to display
            pbrNextCharTimer = new ProgressBar();
            pbrNextCharTimer.Location = new Point(rtbNextChar.Location.X, rtbNextChar.Location.Y+rtbNextChar.Height);
            pbrNextCharTimer.Size = new Size(rtbNextChar.Width, 15);
            this.Controls.Add(pbrNextCharTimer);
            webcam.Location = new Point(webcam.Location.X, webcam.Location.Y + webcam.Height + pbrNextCharTimer.Height);
            webcam.Visible = false;
        }

        // Handle button presses to modify text
        private void btTextModifier_Click(object sender, EventArgs e)
        {
            string buttonType = (sender as Button).Text;
            // Modify the text to be Bold, Italic, or Underlined
            switch (buttonType)
            {
                case "B":
                    rtbTextField.SelectionFont = new Font("Times New Roma", 24, FontStyle.Bold);
                    break;
                case "I":
                    rtbTextField.SelectionFont = new Font("Times New Roma", 24, FontStyle.Italic);
                    break;
                case "U":
                    rtbTextField.SelectionFont = new Font("Times New Roma", 24, FontStyle.Underline);
                    break;
            }
        }

        // Save the text to the currently open file, if there is one
        private void btSave_Click(object sender, EventArgs e)//SETH CODE
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            rtbTextField.SaveFile(currentFile, RichTextBoxStreamType.RichText);
        }

        // Save the text to a new file location
        private void btSaveAs_Click(object sender, EventArgs e)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SaveFileDialog fileSaver = new SaveFileDialog();
            fileSaver.Filter = "Rich Text|*.rtf";
            fileSaver.Title = "Save text";
            fileSaver.RestoreDirectory = true;
            if (fileSaver.ShowDialog() == DialogResult.OK)
            {
                rtbTextField.SaveFile(fileSaver.FileName, RichTextBoxStreamType.RichText);
                currentFile = fileSaver.FileName;
                btSave.Enabled = true;
            }
        }

        // Open text from a new file location
        private void btOpen_Click(object sender, EventArgs e)
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            OpenFileDialog fileOpener = new OpenFileDialog();
            fileOpener.Filter = "Rich Text|*.rtf";
            fileOpener.Title = "Save text";
            fileOpener.RestoreDirectory = true;
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                // rtbTextField.Text = System.IO.File.ReadAllText(fileOpener.FileName);
                rtbTextField.LoadFile(fileOpener.FileName, RichTextBoxStreamType.RichText);
                btSave.Enabled = true;
            }
        }

        // Handle changing the color based on the selection of the combo box
        private void cbColors_SelectionChangeCommitted(object sender, EventArgs e)
        {
            switch ((sender as ComboBox).SelectedItem)
            {
                case "Red":
                    rtbTextField.SelectionColor = Color.Red;
                    break;
                case "Black":
                    rtbTextField.SelectionColor = Color.Black;
                    break;
                case "Blue":
                    rtbTextField.SelectionColor = Color.Blue;
                    break;
                case "Green":
                    rtbTextField.SelectionColor = Color.Green;
                    break;
            }
        }

        // Recieve the signed letter from the Python app
        private void Webcam_GetCurrentSignedSymbol(object sender, string symbol)
        {
            // If the symbol is not the letter 'n', signifiying none, then begin the process to print the letter to the text field
            if (writable && symbol != "n")
            {
                Console.WriteLine("Recieved signed symbol " + symbol + " in quiz");
                // Change the letter previewed to be printed if it's different from the letter currently expected
                if (symbol[0] != nextChar)
                    Task.Run(DelayNextSymbol);
                // Save the next projected letter to be printed
                nextChar = symbol[0];
            }
            else
            {
                // Empty the next projected letter to be printed since none was signed
                rtbNextChar.Text = "";
            }
            if (writable)
                nextChar = symbol[0];
        }

        // Method to delay the printing of the letter to the text field so the user has time to cancel it or doesn't accidentally print a wrong symbol
        private async Task DelayNextSymbol()
        {
            // Stop the program from duplicating the current character by locking the method body from being executed if another thread is currently executing it
            await lockGestureDelay_Semaphore.WaitAsync();
            try {
                char thisChar = nextChar;
                if (thisChar == 'n' || thisChar == ' ')
                    return;
                rtbNextChar.Text = "\n" + thisChar;
                pbrNextCharTimer.Value = 0;
                // Used in equations to make the delay on text being printed modular
                int seconds = 2;
                int miliseconds = 500;
                // Determine how many times the loop should run to reach the number of seconds
                int range = (seconds * (int)(1000 / miliseconds));
                for (int i = 1; i < range+1; i++)
                {
                    // If the character that is supposed to be printed is no longer being signed, cancel printing it
                    if (thisChar != nextChar)
                    {
                        pbrNextCharTimer.Value = 0;
                        rtbNextChar.Text = "";
                        break;
                    }
                    // Wait before continuing
                    await Task.Delay(miliseconds);
                    // Check again if the printing of the character should be cancelled after waiting
                    if (thisChar != nextChar)
                    {
                        pbrNextCharTimer.Value = 0;
                        rtbNextChar.Text = "";
                        break;
                    }
                    // Increase the value of the progress bar
                    pbrNextCharTimer.Value = i * (int)(100/(range));
                    // Print the character to the text field when reaching the end of the loop.
                    if (i == range)
                    {
                        rtbTextField.SelectionLength = 0;
                        rtbTextField.SelectedText = "" + thisChar;
                    }
                }
                // Reset the projected character.
                pbrNextCharTimer.Value = 0;
                nextChar = 'n';
                }
            finally
            {
                // Release the lock on the method body even if an error is encountered
                lockGestureDelay_Semaphore.Release();
            }
        }

        // Begin closing the user control safely
        private void btExit_Click(object sender, EventArgs e)
        {
            writable = false;
            webcam.Exit();
            UCExit?.Invoke(this, e);
        }
    }
}
