using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace KeystrokeDynamics {
    public partial class TypingTest : Form {
        private ITestSelectionMenu testRef;

        private bool[] pastError;
        private List<string> words;
        
        private int backspaceCount = 0;
        private int lastWordIndex = -1;
        private int countdownSeconds = 60;

        private double cpm = 0;
        private double accuracy = 0;
        private double wordCount = 0;
        private double charCount = 0;
        private double avgHoldTime = 0;
        private double avgSeekTime = 0;
        private double wordCorrectCount = 0;
        private double charErrorCount = 0;

        private Timer countdownTimer;
        private TimeSpan seekTime;
        private TimeSpan holdTime;
        private DateTime timeStamp;
        private Stopwatch holdTimer = new Stopwatch();
        private Stopwatch seekTimer = new Stopwatch();
        private Stopwatch sessionTimer = new Stopwatch();
        private List<TimeSpan> holdTimes = new List<TimeSpan>();
        private List<TimeSpan> seekTimes = new List<TimeSpan>();

        public TypingTest(ITestSelectionMenu test) {
            InitializeComponent();
            testRef = test;

            countdownTimer = new Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        private void KeyTest_FormClosed(object sender, FormClosedEventArgs e) {
            testRef.ShowTestSelection();
            countdownTimer.Stop();
            sessionTimer.Stop();
        }

        private void pbClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void KeyTest_Load(object sender, EventArgs e) {
            words = ReadWordsFromFile(Program.wordsFilePath);
            lblTimer.Text = countdownSeconds.ToString("0s");
            lblTimer.Hide();
            rtbDisplay.Hide();
            tbInput.Hide();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e) {
            countdownSeconds -= 1;
            lblTimer.Text = countdownSeconds.ToString("0s");

            if (countdownSeconds <= 0) {
                countdownTimer.Stop();
                sessionTimer.Stop();
                tbInput.Hide();
                rtbDisplay.Hide();
                lblTimer.Hide();

                MessageBox.Show("Test Finished.");

                DialogResult result = MessageBox.Show("Would You Like To Save This Session?", "Test Finished", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    timeStamp = DateTime.UtcNow;
                    SaveSessionToDB(Program.currentUserID);
                } else {
                    this.Close();
                }
            }
        }

        private void tbInput_TextChanged(object sender, EventArgs e) {
            UpdateLabelColour();
        }

        private void tbInput_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Space) {
                if (tbInput.Text.TrimEnd().Equals(rtbDisplay.Text)) {
                    tbInput.Clear();
                    e.Handled = true;
                    wordCount++;
                    wordCorrectCount++;
                    SetRandomWord();
                } else {
                    wordCount++;
                }
            }

            if (e.KeyChar == (char)Keys.Back) {
                backspaceCount++;
            }

            charCount++;
            UpdateMetrics();
        }

        private void tbInput_KeyDown(object sender, KeyEventArgs e) {

            if (!holdTimer.IsRunning) {
                holdTimer.Start();
            }

            if (seekTimer.IsRunning) {
                seekTimer.Stop();
                seekTime = seekTimer.Elapsed;
                if (seekTime.TotalSeconds < 1) {
                    seekTimes.Add(seekTime);
                }
                UpdateMetrics();
                seekTimer.Reset();
            }
        }

        private void tbInput_KeyUp(object sender, KeyEventArgs e) {

            if (holdTimer.IsRunning) {
                holdTimer.Stop();
                holdTime = holdTimer.Elapsed;
                if (holdTime.TotalSeconds < 1) {
                    holdTimes.Add(holdTime);
                }
                UpdateMetrics();
                holdTimer.Reset();
            }

            if (!seekTimer.IsRunning) {
                seekTimer.Start();
            }
        }

        private void SetRandomWord() {
            Random rand = new Random();
            int randIndex;
            do {
                randIndex = rand.Next(words.Count);
            } while (randIndex == lastWordIndex);

            lastWordIndex = randIndex;
            rtbDisplay.Text = words[randIndex];
            rtbDisplay.SelectionAlignment = HorizontalAlignment.Center;
            rtbDisplay.ForeColor = Color.Gray;

            pastError = new bool[rtbDisplay.Text.Length];
            for (int i = 0; i < pastError.Length; i++) {
                pastError[i] = false;
            }
        }

        private void UpdateLabelColour() {
            string enteredText = tbInput.Text;
            string word = rtbDisplay.Text;

            int enteredLength = enteredText.Length;

            rtbDisplay.SelectionStart = 0;
            rtbDisplay.SelectionLength = word.Length;
            rtbDisplay.SelectionColor = Color.Gray;

            for (int i = 0; i < enteredLength; i++) {
                if (i < word.Length) {
                    if (enteredText[i] == word[i]) {
                        rtbDisplay.SelectionStart = i;
                        rtbDisplay.SelectionLength = 1;
                        rtbDisplay.SelectionColor = ColorTranslator.FromHtml("#E9B61F");
                    } else {
                        rtbDisplay.SelectionStart = i;
                        rtbDisplay.SelectionLength = 1;
                        rtbDisplay.SelectionColor = Color.Red;
                        if (i == enteredLength - 1) {
                            if (pastError[i] == false) {
                                pastError[i] = true;
                                charErrorCount++;
                            } else {
                                pastError[i] = false;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateMetrics() {
            accuracy = Math.Round((wordCorrectCount / wordCount) * 100, 2);
            avgHoldTime = holdTimes.Any() ? holdTimes.Average(time => time.TotalSeconds) : 0;
            avgSeekTime = seekTimes.Any() ? seekTimes.Average(time => time.TotalSeconds) : 0;
            cpm = (charCount / sessionTimer.Elapsed.TotalSeconds) * 60;
        }

        private List<string> ReadWordsFromFile(string filePath) {
            List<string> wordsList = new List<string>();

            try {
                using (StreamReader sr = new StreamReader(filePath)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        string[] lineWords = line.Split(' ');
                        wordsList.AddRange(lineWords);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error reading the file: {ex.Message}", "File Reading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return wordsList;
        }

        private void SaveSessionToDB(int currentUserID) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "INSERT INTO [dbo].[KeyData] (UserId, TimeStamp, Accuracy, BackspaceCount, Errors, AvgHoldTime, AvgSeekTime, CPM) " +
                                   "VALUES (@UserId, @TimeStamp, @Accuracy, @BackspaceCount, @Errors, @AvgHoldTime, @AvgSeekTime, @CPM)";

                    using (SqlCommand command = new SqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@UserId", currentUserID);
                        command.Parameters.AddWithValue("@TimeStamp", timeStamp);
                        command.Parameters.AddWithValue("@Accuracy", accuracy);
                        command.Parameters.AddWithValue("@BackspaceCount", backspaceCount);
                        command.Parameters.AddWithValue("@Errors", charErrorCount);
                        command.Parameters.AddWithValue("@AvgHoldTime", avgHoldTime);
                        command.Parameters.AddWithValue("@AvgSeekTime", avgSeekTime);
                        command.Parameters.AddWithValue("@CPM", cpm);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Session Saved Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (Program.currentUser.Equals("Anon")) {
                    testRef.MarkCompleted("typing");
                }

                this.Close();

            } catch (Exception ex) {
                MessageBox.Show("Error Saving Session To The Database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            countdownTimer.Start();
            sessionTimer.Start();
            SetRandomWord();
            pbInfo.Hide();
            lblTimer.Show();
            rtbDisplay.Show();
            tbInput.Show();
            tbInput.Select();
            Cursor.Position = new Point(1000, 800);
        }

        private void pbInfo_Click(object sender, EventArgs e) {
            countdownTimer.Start();
            sessionTimer.Start();
            SetRandomWord();
            pbInfo.Hide();
            lblTimer.Show();
            rtbDisplay.Show();
            tbInput.Show();
            tbInput.Select();
            Cursor.Position = new Point(1000, 800);
        }
    }
}
