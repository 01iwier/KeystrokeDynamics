using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class MainMenu : Form {
        private LaunchScreen loadS;

        public MainMenu(LaunchScreen ls) {
            InitializeComponent();
            this.loadS = ls;
            LoadForm();
        }

        

        private void LoadForm() {
            UpdateComboBox();
        }

        private void MainMenu_FormClosed(object sender, FormClosedEventArgs e) {
            loadS.Close();
        }

        private void MainMenu_Load(object sender, EventArgs e) {

        }

        private void MainMenu_Activated(object sender, EventArgs e) {
            UpdateComboBox();
        }

        public void ShowMainMenu() {
            this.Show();
        }

        private void btnNewProfile_Click(object sender, EventArgs e) {
            this.Hide();
            NewUserForm ns = new NewUserForm(this);
            ns.Show();
        }

        private void btnUpdateProfile_Click(object sender, EventArgs e) {
            if (cbProfiles.SelectedIndex != -1) {

                string enteredPassword = PromptForPassword();

                if (IsPasswordCorrect(enteredPassword, Program.currentUser)) {
                    Program.currentUser = cbProfiles.SelectedItem.ToString();

                    int selectedUserID = GetUserIDFromDatabase(Program.currentUser);

                    if (selectedUserID != -1) {
                        Program.currentUserID = selectedUserID;
                        this.Hide();
                        TestSelectionMenu test = new TestSelectionMenu(this);
                        test.Show();
                    } else {
                        MessageBox.Show("Error Retrieving UserID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                } else if (enteredPassword == "") {
                } else {
                    MessageBox.Show("Incorrect Password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            } else {
                MessageBox.Show("Please Select A Valid Profile");
            }
        }

        /// 
        /// 
        /// 

        private string PromptForPassword() {
            using (var form = new Form()) {
                form.FormBorderStyle = FormBorderStyle.FixedToolWindow; // Remove the border from the form
                form.Text = "Enter Password";

                var lbl = new Label() { Text = "Enter Password", Font = new Font("Futura Heavy", 18f), AutoSize = true, ForeColor = Color.FromArgb(226, 183, 20) };
                lbl.Anchor = AnchorStyles.None;
                
                var passwordBox = new TextBox() { PasswordChar = '*', Name = "EnterPassword" };
                passwordBox.Anchor = AnchorStyles.None;
                passwordBox.Width = 200;
                passwordBox.Height = 30;

                var okButton = new Button() { Text = "OK", Width = 100 };
                okButton.Anchor = AnchorStyles.None;
                okButton.BackColor = SystemColors.Control;
                
                var tableLayoutPanel = new TableLayoutPanel();
                tableLayoutPanel.Dock = DockStyle.Fill;
                tableLayoutPanel.BackColor = Color.FromArgb(50, 52, 55);
                tableLayoutPanel.ColumnCount = 1;
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)) ;
                tableLayoutPanel.RowCount = 5;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // Added padding between label and password box
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // Added padding between label and password box
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Height of password box
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Auto-size for OK button
                tableLayoutPanel.Controls.Add(lbl, 0, 1);
                tableLayoutPanel.Controls.Add(passwordBox, 0, 3);
                tableLayoutPanel.Controls.Add(okButton, 0, 4);

                okButton.Click += (sender, e) => { form.DialogResult = DialogResult.OK; };

                form.Controls.Add(tableLayoutPanel);
                form.Width = 300;
                form.Height = 200;
                form.StartPosition = FormStartPosition.CenterScreen;

                if (form.ShowDialog() == DialogResult.OK) {
                    return passwordBox.Text;
                } else {
                    return "";
                }

            }
        }

        private bool IsPasswordCorrect(string enteredPassword, string userName) {
            bool isCorrect = false;
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "SELECT PasswordHash FROM [dbo].[Users] WHERE UserName = @UserName";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserName", userName);
                    string storedHash = command.ExecuteScalar()?.ToString();

                    if (storedHash != null) {
                        isCorrect = (HashPassword(enteredPassword) == storedHash);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Error Retrieving Password Hash: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return isCorrect;
        }

        private string HashPassword(string password) {
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes) {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// 
        /// 
        /// 
    
        private void btnIdentify_Click(object sender, EventArgs e) {
            /*if (IsCurrentUserInCombinedUserData()) {*/
                Program.currentUser = "Anon";
                Program.currentUserID = 11;
                this.Hide();
                AnonTestSelectionMenu test = new AnonTestSelectionMenu(this);
                test.Show();
           /* } else {
                MessageBox.Show("Error: Current user's ID not found in CombinedUserData.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private bool IsCurrentUserInCombinedUserData() {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "SELECT UserID FROM [dbo].[CombinedUserData] WHERE UserName = @UserName";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserName", Program.currentUser);

                    object result = command.ExecuteScalar();

                    return (result != null && result != DBNull.Value);
                }
            } catch (Exception ex) {
                MessageBox.Show("Error Checking UserID in CombinedUserData: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void cbProfiles_SelectedIndexChanged(object sender, EventArgs e) {
            if (cbProfiles.SelectedIndex != -1) {
                Program.currentUser = cbProfiles.SelectedItem.ToString();
            }

            Console.WriteLine(Program.currentUser.ToString());
        }

        private void UpdateComboBox() {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "SELECT UserName FROM [dbo].[Users]";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    cbProfiles.Items.Clear();

                    foreach (DataRow row in table.Rows) {
                        string userName = row["UserName"].ToString();
                        if (!userName.Equals("Anon")) {
                            cbProfiles.Items.Add(userName);
                        }
                    }
                    if (!string.IsNullOrEmpty(Program.currentUser) && cbProfiles.Items.Contains(Program.currentUser)) {
                        cbProfiles.SelectedItem = Program.currentUser;
                    }

                }
            } catch (Exception ex) {
                MessageBox.Show("Error Updating Profiles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetUserIDFromDatabase(string userName) {
            int userID = -1;
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "SELECT UserID FROM [dbo].[Users] WHERE UserName = @UserName";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserName", userName);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value) {
                        userID = Convert.ToInt32(result);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Error Retrieving UserID:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return userID;
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
