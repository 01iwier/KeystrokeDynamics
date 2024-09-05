using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class AnonTestSelectionMenu : Form, ITestSelectionMenu {
        private MainMenu mainMenuRef;

        public AnonTestSelectionMenu(MainMenu mainMenu) {
            InitializeComponent();
            DeleteUserRecords(Program.currentUserID);
            mainMenuRef = mainMenu;
        }

        public void ShowMainMenu() {
            mainMenuRef.Show();
        }

        public void ShowTestSelection() {
            this.Show();
        }

        public void MarkCompleted(string type) {
            if (type.Equals("typing")) {
                cbType.Checked = true;
            } else if (type.Equals("mouse")) {
                cbMouse.Checked = true;
            }
        }

        private void AnonTestSelectionMenu_FormClosed(object sender, FormClosedEventArgs e) {
            ShowMainMenu();
        }

        private void btnType_Click(object sender, EventArgs e) {
            this.Hide();
            TypingTest kt = new TypingTest(this);
            kt.Show();
        }

        private void btnMouse_Click(object sender, EventArgs e) {
            this.Hide();
            MouseTest mt = new MouseTest(this);
            mt.Show();
        }

        private void pbClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private double CalculateEuclideanDistance(double[] vector1, double[] vector2) {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length.");

            double sum = 0.0;
            for (int i = 0; i < vector1.Length; i++) {
                sum += Math.Pow(vector1[i] - vector2[i], 2);
            }

            return Math.Sqrt(sum);
        }

        private void btnIdentify_Click(object sender, EventArgs e) {
            if (cbMouse.Checked == true && cbType.Checked == true) {
                try {
                    int anonymousUserId = 11;
                    using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                        connection.Open();
                        string anonymousUserQuery = "SELECT * FROM [dbo].[CombinedUserData] WHERE UserId = @UserId";
                        using (SqlCommand anonymousUserCommand = new SqlCommand(anonymousUserQuery, connection)) {
                            anonymousUserCommand.Parameters.AddWithValue("@UserId", anonymousUserId);

                            double avgSingleClick = 0.0, avgDoubleClick = 0.0, avgScroll = 0.0, avgHold = 0.0, count = 0.0, accuracy = 0.0, backspaceCount = 0.0, errors = 0.0, avgHoldTime = 0.0, avgSeekTime = 0.0, cpm = 0.0;

                            using (SqlDataReader reader = anonymousUserCommand.ExecuteReader()) {
                                if (reader.Read()) {
                                    avgSingleClick = Convert.ToDouble(reader["AvgSingleClick"]);
                                    avgDoubleClick = Convert.ToDouble(reader["AvgDoubleClick"]);
                                    avgScroll = Convert.ToDouble(reader["AvgScroll"]);
                                    avgHold = Convert.ToDouble(reader["AvgHold"]);
                                    count = Convert.ToDouble(reader["Count"]);
                                    accuracy = Convert.ToDouble(reader["Accuracy"]);
                                    backspaceCount = Convert.ToDouble(reader["BackspaceCount"]);
                                    errors = Convert.ToDouble(reader["Errors"]);
                                    avgHoldTime = Convert.ToDouble(reader["AvgHoldTime"]);
                                    avgSeekTime = Convert.ToDouble(reader["AvgSeekTime"]);
                                    cpm = Convert.ToDouble(reader["AvgCPM"]);
                                }
                            }

                            int closestUserId = -1;
                            double minDistance = double.MaxValue;
                            string allUsersQuery = "SELECT * FROM [dbo].[CombinedUserData]";
                            using (SqlCommand allUsersCommand = new SqlCommand(allUsersQuery, connection)) {
                                using (SqlDataReader userReader = allUsersCommand.ExecuteReader()) {
                                    while (userReader.Read()) {
                                        int currentUserID = Convert.ToInt32(userReader["UserId"]);
                                        if (currentUserID == 11) {
                                            continue;
                                        }

                                        double[] currentUserFeatures = new double[] {
                                    Convert.ToDouble(userReader["AvgSingleClick"]),
                                    Convert.ToDouble(userReader["AvgDoubleClick"]),
                                    Convert.ToDouble(userReader["AvgScroll"]),
                                    Convert.ToDouble(userReader["AvgHold"]),
                                    Convert.ToInt32(userReader["Count"]),
                                    Convert.ToDouble(userReader["Accuracy"]),
                                    Convert.ToInt32(userReader["BackspaceCount"]),
                                    Convert.ToInt32(userReader["Errors"]),
                                    Convert.ToDouble(userReader["AvgHoldTime"]),
                                    Convert.ToDouble(userReader["AvgSeekTime"]),
                                    Convert.ToDouble(userReader["AvgCPM"])
                                        };

                                        double distance = CalculateEuclideanDistance(new double[] { avgSingleClick, avgDoubleClick, avgScroll, avgHold, count, accuracy, backspaceCount, errors, avgHoldTime, avgSeekTime, cpm }, currentUserFeatures);

                                        if (distance < minDistance) {
                                            minDistance = distance;
                                            closestUserId = Convert.ToInt32(userReader["UserId"]);
                                        }
                                    }
                                }
                            }

                            MessageBox.Show($"Closest match: " + GetUsernameById(closestUserId));
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Error identifying user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            } else {
                MessageBox.Show("Complete both tests before continuing.");
            }
        }

        public string GetUsernameById(int userId) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();

                    string query = "SELECT UserName FROM [dbo].[Users] WHERE UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@UserId", userId);

                        object result = command.ExecuteScalar();
                        if (result != null) {
                            return result.ToString();
                        }
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Error retrieving username: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private void DeleteUserRecords(int userId) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();

                    string deleteMouseDataQuery = "DELETE FROM [dbo].[MouseData] WHERE UserId = @UserId";
                    using (SqlCommand deleteMouseDataCommand = new SqlCommand(deleteMouseDataQuery, connection)) {
                        deleteMouseDataCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteMouseDataCommand.ExecuteNonQuery();
                    }
                    
                    string deleteKeyDataQuery = "DELETE FROM [dbo].[KeyData] WHERE UserId = @UserId";
                    using (SqlCommand deleteKeyDataCommand = new SqlCommand(deleteKeyDataQuery, connection)) {
                        deleteKeyDataCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteKeyDataCommand.ExecuteNonQuery();
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Error deleting user records: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
