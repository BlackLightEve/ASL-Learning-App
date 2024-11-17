namespace ASLLearningApp
{
    public partial class Form1 : Form
    {

        private List<Button> currentLayout;
        private UserControl currentControl;
        private QuizSelectionMenu currentQuizSelectionMenu;
        private FreeWriteMenu currentFreeWriteMenu;
        private LearningMenu currentLearningMenu;
        private Button btMenu;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            Setup();
        }

        // Setup the application from default. Define scaling and absolute position.
        private void Setup()
        {
            this.Text = "Main Menu";
            this.WindowState = FormWindowState.Maximized;
            Button btLearn = new Button(), btWrite = new Button(), btQuiz = new Button();
            List<Button> localButtons = new List<Button>() { btLearn, btWrite, btQuiz };
            int xConstant = this.Size.Width / 5;
            int yConstant = this.Size.Height / 5;
            int padding = 10;
            int posX = 0;
            // Create the three buttons to select what section of the program the user wants to access.
            btLearn.Text = "Learn";
            btWrite.Text = "Write";
            btQuiz.Text = "Quiz";
            foreach (Button bt in localButtons)
            {
                posX += xConstant;
                bt.Location = new Point(posX, yConstant);
                bt.Size = new Size(xConstant - padding, xConstant - padding);
                bt.Font = new Font("Arial", 24, FontStyle.Bold);
                bt.Enabled = true;
                this.Controls.Add(bt);
                bt.Click += new EventHandler(btMenuSelect_Click);
                bt.FlatStyle = FlatStyle.Flat;
            }
            // Create a button to go to the webcam settings.
            Button btWebcamSettings = new Button();
            btWebcamSettings.Location = new Point(posX, localButtons[2].Location.Y + localButtons[2].Height + padding);
            btWebcamSettings.Size = new Size(xConstant - padding, xConstant/2);
            btWebcamSettings.Text = "Webcam Settings";
            btWebcamSettings.Font = new Font("Arial", 24, FontStyle.Bold);
            btWebcamSettings.Enabled = true;
            btWebcamSettings.FlatStyle = FlatStyle.Flat;
            currentLayout = localButtons;
            // Create a button to go to the main menu that can be used later.
            btMenu = new Button();
            btMenu.Dock = DockStyle.Bottom;
            btMenu.Size = new Size(btMenu.Size.Width, 75);
            btMenu.Click += new EventHandler(btMenu_Click);
            btMenu.Text = "Menu";
            btMenu.Font = new Font("Arial", 24, FontStyle.Bold);
            btMenu.FlatStyle = FlatStyle.Flat;
            this.Controls.Add(btMenu);
            btMenu.Hide();
        }

        // Handler for when the user clicks a button to traverse to another menu.
        private void btMenuSelect_Click(object sender, EventArgs e)
        {
            string buttonType = (sender as Button).Text;
            UserControl uc = new UserControl();
            switch (buttonType)
            {
                case "Quiz":
                    NewQuizSelectionMenu();
                    break;
                case "Learn":
                    NewLearningMenu();
                    break;
                case "Write":
                    NewFreeWriteMenu();
                    break;
            }
            this.Text = (sender as Button).Text;
        }

        // Create a new quiz selection menu.
        private void NewQuizSelectionMenu()
        {
            currentQuizSelectionMenu = new QuizSelectionMenu();
            ChangeMenu(currentQuizSelectionMenu);
            currentQuizSelectionMenu.UCExit += quizSelectionMenu_exited;
        }

        // Create a new learning menu.
        private void NewLearningMenu()
        {
            currentLearningMenu = new LearningMenu();
            ChangeMenu(currentLearningMenu);
            currentLearningMenu.UCExit += learningMenu_exited;
        }

        // Create a new free writing menu.
        private void NewFreeWriteMenu()
        {
            currentFreeWriteMenu = new FreeWriteMenu();
            ChangeMenu(currentFreeWriteMenu);
            currentFreeWriteMenu.UCExit += freeWriteMenu_exited;
        }

        // Handler for the menu button being clicked.
        private void btMenu_Click(object sender, EventArgs e)
        {
            btMenu.Hide();
            foreach (Button bt in currentLayout)
                bt.Show();
            this.Text = "Main Menu";
            currentControl.Dispose();
        }

        // Method to hide the main menu when traversing to another menu.
        private void ChangeMenu(UserControl uc)
        {
            uc.Location = new Point(200, 200);
            uc.Dock = DockStyle.Fill;
            uc.BringToFront();
            this.Controls.Add(uc);
            foreach (Button bt in currentLayout)
                bt.Hide();
            currentControl = uc;
        }

        // Method to unhide the main menu when coming back to it.
        private void ShowButtons()
        {
            this.Text = "Main Menu";
            foreach (Button bt in currentLayout)
                bt.Show();
            currentControl.Dispose();
        }

        // Handlers for each of the menu buttons being exited.
        private void quizSelectionMenu_exited(object sender, EventArgs e)
        {
            ShowButtons();
        }

        private void learningMenu_exited(object sender, EventArgs e)
        {
            ShowButtons();
        }

        private void freeWriteMenu_exited(object sender, EventArgs e)
        {
            ShowButtons();
        }
    }
}
