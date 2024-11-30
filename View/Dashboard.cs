using System;
using System.Windows.Forms;
using System.Drawing;

namespace TravelPal.UI
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Dashboard";
            this.ClientSize = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var welcomeLabel = new Label()
            {
                Text = "Welcome to TravelPal!",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(150, 150),
                AutoSize = true
            };

            this.Controls.Add(welcomeLabel);
        }
    }
}
