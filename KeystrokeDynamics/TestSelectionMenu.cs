using System;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class TestSelectionMenu : Form, ITestSelectionMenu {
        private MainMenu mainMenuRef;

        public TestSelectionMenu(MainMenu mainMenu) {
            InitializeComponent();
            mainMenuRef = mainMenu;
        }

        public void ShowMainMenu() {
            mainMenuRef.Show();
        }

        public void ShowTestSelection() {
            this.Show();
        }

        public void MarkCompleted(string type) {
            Console.WriteLine(type);
        }

        private void TestSelectionMenu_FormClosed(object sender, FormClosedEventArgs e) {
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

        private void lblTitle_Click(object sender, EventArgs e) {
            Console.WriteLine(Program.currentUser.ToString() + "    " + Program.currentUserID.ToString());
        }
    }
}
