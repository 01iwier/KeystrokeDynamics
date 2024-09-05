using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class MouseTest : Form {

        List<PictureBox> items = new List<PictureBox>();
        private DateTime mouseButtonDownTime;
        private DateTime timeStamp;
        private ITestSelectionMenu testRef;
        Random rand = new Random();
        int counter = 0;
        int timer = 30;

        private List<long> singleClickTimes = new List<long>();
        private List<long> doubleClickTimes = new List<long>();
        private List<long> scrollTimes = new List<long>();
        private List<long> holdTimes = new List<long>();
        private DateTime targetAppearanceTime;

        double avgSingleClickTime = 0;
        double avgDoubleClickTime = 0;
        double avgScrollTime = 0;
        double avgHoldTime = 0;

        public MouseTest(ITestSelectionMenu test) {
            InitializeComponent();
            testRef = test;
        }

        private void MouseTest_Load(object sender, EventArgs e) {
            UpdateTimerLabel();
            timer1.Enabled = false;
            lblCount.Text = "Count: 0";
            lblTime.Hide();
            lblCount.Hide();
        }

        private void MouseTest_FormClosed(object sender, FormClosedEventArgs e) {
            testRef.ShowTestSelection();
        }

        private void pbClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void UpdateTimerLabel() {
            lblTime.Text = timer.ToString("00:00");
        }

        private void RemoveAllTargets() {
            foreach (PictureBox pictureBox in items) {
                this.Controls.Remove(pictureBox);
            }

            items.Clear();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (timer <= 0) {
                timer1.Enabled = false;
                UpdateTimerLabel();
                RemoveAllTargets();
                avgSingleClickTime = singleClickTimes.Count > 0 ? singleClickTimes.Average() / TimeSpan.TicksPerMillisecond : 0;
                avgDoubleClickTime = doubleClickTimes.Count > 0 ? doubleClickTimes.Average() / TimeSpan.TicksPerMillisecond : 0;
                avgScrollTime = scrollTimes.Count > 0 ? scrollTimes.Average() / TimeSpan.TicksPerMillisecond : 0;
                avgHoldTime = holdTimes.Count > 0 ? holdTimes.Average() / TimeSpan.TicksPerMillisecond : 0;
                this.Close();

                MessageBox.Show($"Test Finished.");

                DialogResult result = MessageBox.Show("Would You Like To Save This Session?", "Test Finished", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes) {
                    timeStamp = DateTime.UtcNow;
                    SaveSessionToDB(Program.currentUserID);
                } else {
                    this.Close();
                }

            } else {
                timer--;
                UpdateTimerLabel();
            }
        }

        private void MakeTarget() {
            targetAppearanceTime = DateTime.Now;
            PictureBox newPic = new PictureBox();
            string imagePath;
            int clicksRequired;
            int targetType = rand.Next(1, 4);
            newPic.MouseDown += NewPic_MouseDown;

            switch (targetType) {
                case 1:
                    imagePath = @"C:\Users\Wieck\source\repos\KeystrokeDynamics\KeystrokeDynamics\targetDouble.png";
                    clicksRequired = 2;
                    newPic.DoubleClick += NewPic_DoubleClick;
                    break;
                case 2:
                    imagePath = @"C:\Users\Wieck\source\repos\KeystrokeDynamics\KeystrokeDynamics\targetSingle.png";
                    clicksRequired = 1;
                    newPic.Click += NewPic_Click;
                    break;
                case 3:
                    imagePath = @"C:\Users\Wieck\source\repos\KeystrokeDynamics\KeystrokeDynamics\targetScroll.png";
                    clicksRequired = 0;
                    newPic.MouseWheel += NewPic_MouseWheel;
                    break;
                default:
                    throw new InvalidOperationException("Invalid target type.");
            }


            newPic.Tag = clicksRequired; 
            newPic.Height = 60;
            newPic.Width = 60;
            newPic.Image = Image.FromFile(imagePath);
            newPic.SizeMode = PictureBoxSizeMode.StretchImage;
            int x = rand.Next(40, this.ClientSize.Width - newPic.Width - 40);
            int y = rand.Next(100, this.ClientSize.Height - newPic.Height - 40);
            newPic.Location = new Point(x, y);
            items.Add(newPic);
            this.Controls.Add(newPic);
        }

        private void NewPic_Click(object sender, EventArgs e) {
            PictureBox temPic = sender as PictureBox;
            long holdDuration = DateTime.Now.Ticks - mouseButtonDownTime.Ticks;
            holdTimes.Add(holdDuration);

            int clicksLeft = (int)temPic.Tag;
            clicksLeft--;

            if (clicksLeft <= 0) {
                items.Remove(temPic);
                this.Controls.Remove(temPic);
                counter++;
                lblCount.Text = "Count: " + counter;
                singleClickTimes.Add(DateTime.Now.Ticks - targetAppearanceTime.Ticks);
                MakeTarget(); 
            } else {
                temPic.Tag = clicksLeft;
            }
        }

        private void NewPic_DoubleClick(object sender, EventArgs e) {
            PictureBox temPic = sender as PictureBox;
            long doubleClickTime = DateTime.Now.Ticks - targetAppearanceTime.Ticks;
            doubleClickTimes.Add(doubleClickTime);

            long holdDuration = DateTime.Now.Ticks - mouseButtonDownTime.Ticks;
            holdTimes.Add(holdDuration);

            int clicksLeft = (int)temPic.Tag;
            clicksLeft -= 2;

            if (clicksLeft <= 0) {
                items.Remove(temPic);
                this.Controls.Remove(temPic);
                counter += 2; 
                lblCount.Text = "Count: " + counter;
                MakeTarget(); 
            } else {
                temPic.Tag = clicksLeft;
            }
        }

        private void NewPic_MouseWheel(object sender, MouseEventArgs e) {
            PictureBox temPic = sender as PictureBox;
            long scrollTime = DateTime.Now.Ticks - targetAppearanceTime.Ticks;
            scrollTimes.Add(scrollTime);
            int clicksLeft = (int)temPic.Tag;
            clicksLeft -= Math.Abs(e.Delta) / 120;

            if (clicksLeft <= 0) {
                items.Remove(temPic);
                this.Controls.Remove(temPic);
                counter++;
                lblCount.Text = "Count: " + counter;
                MakeTarget(); 
            } else {
                temPic.Tag = clicksLeft;
            }
        }

        private void NewPic_MouseDown(object sender, MouseEventArgs e) {
            mouseButtonDownTime = DateTime.Now;
        }

        private void SaveSessionToDB(int currentUserID) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "INSERT INTO [dbo].[MouseData] (UserId, TimeStamp, AvgSingleClick, AvgDoubleClick, AvgScroll, AvgHold, Count) " +
                                   "VALUES (@UserId, @TimeStamp, @AvgSingleClick, @AvgDoubleClick, @AvgScroll, @AvgHold, @Count)";

                    using (SqlCommand command = new SqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@UserId", currentUserID);
                        command.Parameters.AddWithValue("@TimeStamp", timeStamp);
                        command.Parameters.AddWithValue("@AvgSingleClick", avgSingleClickTime);
                        command.Parameters.AddWithValue("@AvgDoubleClick", avgDoubleClickTime);
                        command.Parameters.AddWithValue("@AvgScroll", avgScrollTime);
                        command.Parameters.AddWithValue("@AvgHold", avgHoldTime);
                        command.Parameters.AddWithValue("@Count", counter);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Session Saved Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (Program.currentUser.Equals("Anon")) {
                    testRef.MarkCompleted("mouse");
                }

                this.Close();

            } catch (Exception ex) {
                MessageBox.Show("Error Saving Session To The Database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            timer1.Enabled = true;
            pbPrep.Hide();
            lblCount.Show();
            lblTime.Show();
            MakeTarget();
        }
    }
}
