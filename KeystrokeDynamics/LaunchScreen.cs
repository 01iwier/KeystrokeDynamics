using System;
using System.Windows.Forms;

namespace KeystrokeDynamics {
    public partial class LaunchScreen : Form {
        private Timer timer;
        private int dotCount = 3;
        private int tickCounter = 0;

        public LaunchScreen() {
            InitializeComponent();
            pbLoadingBar.Width = 0;
            timer = new Timer() { Interval = 16 };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            if (pbLoadingBar.Width == 440) {
                this.Hide();
                MainMenu main = new MainMenu(this);
                main.Show();
                pbLoadingBar.Width += 1;
            } else {
                tickCounter++;
                pbLoadingBar.Width += 11;
                if (tickCounter % 6 == 0) { UpdateLoadingLabel(); }
                if (pbLoadingBar.Width > 380) { this.Opacity -= 0.08; }
            }
        }

        private void UpdateLoadingLabel() {
            dotCount = (dotCount + 1) % 4;
            string loadingText = "Loading";
            for (int i = 0; i < dotCount; i++) { loadingText += "."; }
            lblLoading.Text = loadingText;
        }
    }
}
