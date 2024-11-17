using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ASLLearningApp
{
    internal class Quiz
    {
        private List<String> questions;
        private int currentQuestion = 0;
        private string diff;
        private int[] strikes;
        private bool[] success;
        private int randomWordNum;
        private Random rnd = new Random();
        private int[] alreadyUsed = new int[5];
        private string word;
        private int count;
        private string[] wordChoice= new string[5];
        private int wordCount = 0;

        // Constructor to create a quiz based on a difficulty selected.
        public Quiz(string difficultySelected)
        {
            diff = difficultySelected;
            questions = new List<String>();
            questions = GetQuestions();
            strikes = new int[] { 0, 0, 0, 0, 0};
            success = new bool[] {false, false, false, false, false};
        }

        // Get all the questions related to each difficulty.
        private List<String> GetQuestions()
        {
            List<String> obtainedQuestions = new List<String>();
            if (diff == "Easy")
            {
                RandomQuestions();
                obtainedQuestions.AddRange(wordChoice);
            }
            else if (diff == "Intermediate")
            {
                RandomQuestions();
                obtainedQuestions.AddRange(wordChoice);
            }
            else
            {
                RandomQuestions();
                obtainedQuestions.AddRange(wordChoice);
            }
            return obtainedQuestions;

        }

        // Get random questions from the files.
        public void RandomQuestions()
        {
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            StreamReader stream = new StreamReader(currentDirectory + "\\words\\" + diff + ".txt");
            do
            {
                // Get a random word from the file that isn't already contained in the list.
                while (true)
                {
                    randomWordNum = rnd.Next(1, 11);
                    if (!alreadyUsed.Contains(randomWordNum))
                    {
                        break;
                    }
                }
                alreadyUsed[wordCount] = randomWordNum;
                count = 0;
                string wordSave = "";
                while ((word = stream.ReadLine()) != null && count < randomWordNum)
                {
                    count++;
                    wordSave = word;

                }
                wordChoice[wordCount] = wordSave;
                wordCount++;
                stream.DiscardBufferedData();
                stream.BaseStream.Seek(0, SeekOrigin.Begin);
            } while (wordCount <= 4);
        }

        // Get the next word in the list if possible.
        public string NextWord()
        {
            if (currentQuestion < questions.Count - 1)
                currentQuestion += 1;
            return questions[currentQuestion];
        }

        // Get the previous word in the list if possible.
        public string PrevWord()
        {
            if (currentQuestion > 0)
                currentQuestion -= 1;
            return questions[currentQuestion];
        }

        // Get the current word.
        public string CurrentWord()
        {
            return questions[currentQuestion];
        }

        // Get the size of the list of words.
        public int Size()
        {
            return questions.Count();
        }

        // Jump to a specified index.
        public string JumpToWord(int index)
        {
            currentQuestion = index;
            return questions[currentQuestion];
        }

        // Get the number of strikes for the current index.
        public int GetStrikes(int index)
        {
            return(strikes[index]);
        }

        // Add strikes to the current currently indexed word, if the user does not already have 3 strikes.
        public void AddStrike(int index)
        {
            if (strikes[index] != 3)
                strikes[index]+=1;
        }


        // Set the word at the current index as correctly spelled.
        public void SuccessfulSpelling(int index)
        {
            success[index] = true;
        }

        // Check if this the word at the current index has been already spelled correctly.
        public bool CheckSuccessful(int index)
        {
            return success[index];
        }

        // Retrieve the grade for this quiz based on questions answered correctly.
        public double GetGrade()
        {
            double points = 100 / success.Length;
            double pointsTotal = 0;
            for (int i = 0; i < success.Length; i++)
            {
                if (success[i] == true)
                    pointsTotal += points;
            }
            return pointsTotal;
        }
    }
}
