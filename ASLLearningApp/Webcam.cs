using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Python.Runtime;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace ASLLearningApp
{
    public partial class Webcam : UserControl
    {
        private bool isRunning = false;
        private bool pipeInUse = false;
        Thread dataReciever;
        // Delegate for the event for when the symbol is changed
        public delegate void SignedSymbolChangedHandler(object sender, string symbol);

        // Event
        public event SignedSymbolChangedHandler SignedSymbolChanged;

        // Webcam Object.
        public Webcam()
        {
            InitializeComponent();
        }

        // Code executing when this user control is loaded.
        private void Webcam_Load(object sender, EventArgs e)
        {
            Setup();
        }

        // Setup the user control.
        private void Setup()
        {
            // Set the isRunning variable to true, a method later will wait for this value to become false before closing the user control.
            isRunning = true;
            Enter();
            // Create a new thread on which to handle running the Python script.
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true; // Set as background thread
                RunScript("WebcamScript");
            }).Start();
            // Create a named pipe server.
            dataReciever = new Thread(ReceiveFingerspelling);
            dataReciever.Start();
        }

        // Method to recieve any finger spelling gesture detected by the Python script.
        private void ReceiveFingerspelling()
        {
            // Similar use to the "isRunning" variable
            pipeInUse = true;
            // Loop continuously until a stop condition is met.
            while (true)
            {
                // Use an IPC pipe to communicate with the Python application.
                using (var pipeServer = new NamedPipeServerStream("GesturePipe", PipeDirection.InOut))
                {
                    pipeServer.WaitForConnection();
                    // Read the result from the pipe.
                    using (var reader = new StreamReader(pipeServer))
                    {
                        // Read the recognized gesture
                        string gesture = reader.ReadLine();
                        // Raise the event to shut down when recieving the '*' sign.
                        if (gesture != "*")
                            SignedSymbolChanged?.Invoke(this, gesture); 
                        else
                            break;
                    }
                }
            }
            // Let the exit method know it can safely close the user control now that the pipe is closed.
            pipeInUse = false;
        }

        // Method to read and execute Python script.
        private void RunScript(string scriptName)
        {
            Console.WriteLine("Webcam script starting (C#)...");
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Construct the path to WebcamScript.py using a relative path
            string webcamScriptPath = Path.Combine(currentDirectory, @"scripts");

            // Set the PythonDLL path
            Runtime.PythonDLL = @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python310\python310.dll";

            // Setup the Python engine to allow the C# app to communicate with the Python script.
            PythonEngine.Initialize();
            dynamic os = Py.Import("os");
            dynamic sys = Py.Import("sys");

            // Add the directory containing the script to sys.path
            sys.path.append(webcamScriptPath);

            isRunning = true;

            // Use the Python Global Interpreter Lock to execute the Python script.
            using (Py.GIL())
            {
                try
                {
                    // Run the Python script.
                    var pythonScript = Py.Import(scriptName);
                    pythonScript.InvokeMethod("runScriptFromCS");
                }
                catch (PythonException ex)
                {
                    MessageBox.Show("An error occurred while running the Python script: " + ex.Message);
                }
            }
            // Shut down the Python engine once the program is finished.
            PythonEngine.Shutdown();
            isRunning = false;
        }

        // This method will allow you to pass the current signed symbol by the user to the other controls (i.e. the quiz).
        public void OnSignedSymbolChanged(Python.Runtime.PyObject symbol)
        {
            string symbolAsString = symbol.ToString();
            SignedSymbolChanged?.Invoke(this, symbolAsString);
        }

        // Method to safely begin the exit of the Python script.
        public void Exit()
        {
            // Writes to a file to communicate to the Python program to shut down.
            string filePath = @".\scripts\MultithreadController.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write("End");
            }
            // Calls a method to wait for both the main Python script and the pipe to shut down and close successfully.
            int result = Task.Run(async () => await WaitForShutdown()).Result;
        }

        public void Enter()
        {
            string filePath = @".\scripts\MultithreadController.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write("");
            }
        }

        // Await shut down of Python script and the pipe.
        public async Task<int> WaitForShutdown()
        {
            if (isRunning)
                System.Threading.SpinWait.SpinUntil(() => !isRunning);
            if (pipeInUse)
                System.Threading.SpinWait.SpinUntil(() => !pipeInUse);
            return (1);
        }
    }
}
