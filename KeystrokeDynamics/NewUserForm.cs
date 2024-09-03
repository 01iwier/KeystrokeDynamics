using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class NewUserForm : Form {
        private MainMenu mainMenuRef;

        public NewUserForm(MainMenu mainMenu) {
            InitializeComponent();
            mainMenuRef = mainMenu;
        }
        private void NewUserForm_FormClosed(object sender, FormClosedEventArgs e) {
            mainMenuRef.ShowMainMenu();
        }

        private void pbClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void NewUserForm_Load(object sender, EventArgs e) {
            //MessageBox.Show("Please enter your name" + Environment.NewLine + "(must start with a capital and only contain letters)." + Environment.NewLine + "Password must be 5 characters or more.");
        }

        

        private bool IsValidName(string name) {
            return !string.IsNullOrWhiteSpace(name) && name.All(char.IsLetter) && char.IsUpper(name.First());
        }

        private bool IsValidPassword(string password) {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 5 && password.Length <= 50;
        }

        private void ClearTextBoxes() {
            txtName.Clear();
            txtPass.Clear();
        }

        private void btnSubmit_Click(object sender, EventArgs e) {
            string enteredName = txtName.Text.Trim();
            string enteredPassword = txtPass.Text;

            if (IsValidName(enteredName) && IsValidPassword(enteredPassword)) {
                if (IsDuplicateName(enteredName)) {
                    MessageBox.Show("A profile with this name already exists. Please choose a different name.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearTextBoxes();
                } else {
                    try {
                        AddNewProfileToDatabase(enteredName, enteredPassword);
                        MessageBox.Show("Profile added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Program.currentUser = enteredName;
                        this.Close();
                    } catch (Exception ex) {
                        MessageBox.Show("Error adding profile to the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            } else {
                MessageBox.Show("Please enter a valid name (one word with no spaces, first letter must be capital), and a password of at least 5 characters.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ClearTextBoxes();
            }
        }

        private void AddNewProfileToDatabase(string name, string password) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string hashedPassword = HashPassword(password);
                    string query = "INSERT INTO [dbo].[Users] (UserName, PasswordHash) VALUES (@UserName, @PasswordHash)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserName", name);
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                MessageBox.Show("Error adding profile to the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDuplicateName(string name) {
            try {
                using (SqlConnection connection = new SqlConnection(Program.connectionString)) {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM [dbo].[Users] WHERE UserName = @UserName";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserName", name);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            } catch (Exception ex) {
                MessageBox.Show("Error checking duplicate names: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

        

        private void lblTitle_Click(object sender, EventArgs e) {
            Console.WriteLine(HashPassword("example"));
        }
    }
}
