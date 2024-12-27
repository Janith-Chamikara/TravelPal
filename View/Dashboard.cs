using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using TravelPal.Sessions;

namespace TravelPal.UI
{
    public partial class DashboardForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private Label welcomeLabel;
        private RoundedButton shortestPathButton;
        private RoundedButton logoutButton;

        private RoundedButton updateProfileButton;
        private RoundedButton showLocationsButton;

        public DashboardForm(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponents();
            LoadUserInfo();
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "TravelPal Dashboard";
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create main container
            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            // Welcome Label
            welcomeLabel = new Label
            {
                Text = "Welcome to TravelPal!",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            // Shortest Path Button
            shortestPathButton = new RoundedButton
            {
                Text = "Find Shortest Path",
                Font = new Font("Segoe UI", 12),
                Size = new Size(200, 40),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderRadius = 8,
                Cursor = Cursors.Hand,
                Margin = new Padding(10)
            };
            shortestPathButton.FlatAppearance.BorderSize = 0;
            shortestPathButton.Click += ShortestPathButton_Click;

            // Update Profile
            updateProfileButton = new RoundedButton
            {
                Text = "Update Profile",
                Font = new Font("Segoe UI", 12),
                Size = new Size(200, 40),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderRadius = 8,
                Cursor = Cursors.Hand,
                Margin = new Padding(10)
            };
            updateProfileButton.Click += UpdateProfileButton_Click;
            //show locations
            showLocationsButton = new RoundedButton
            {
                Text = "Show Locations",
                Font = new Font("Segoe UI", 12),
                Size = new Size(200, 40),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderRadius = 8,
                Cursor = Cursors.Hand,
                Margin = new Padding(10)
            };
            showLocationsButton.Click += ShowLocationsButton_Click;

            // Logout Button
            logoutButton = new RoundedButton
            {
                Text = "Logout",
                Font = new Font("Segoe UI", 12),
                Size = new Size(200, 40),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderRadius = 8,
                Cursor = Cursors.Hand
            };
            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.Click += LogoutButton_Click;

            // Create button container for centering
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            buttonPanel.Controls.Add(updateProfileButton);
            buttonPanel.Controls.Add(shortestPathButton);
            buttonPanel.Controls.Add(showLocationsButton);

            // Create logout button container
            FlowLayoutPanel logoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10)
            };
            logoutPanel.Controls.Add(logoutButton);

            // Add controls to main panel
            mainPanel.Controls.Add(welcomeLabel, 0, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);
            mainPanel.Controls.Add(logoutPanel, 0, 2);

            // Set row styles
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            // Add main panel to form
            this.Controls.Add(mainPanel);
        }

        private void LoadUserInfo()
        {
            if (!UserSession.Instance.IsLoggedIn())
            {
                MessageBox.Show("Please log in first", "Session Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                ShowLoginForm();
                return;
            }

            welcomeLabel.Text = $"Welcome, {UserSession.Instance.Username}!";
        }

        private void ShortestPathButton_Click(object sender, EventArgs e)
        {
            try
            {
                var shortestPathView = _serviceProvider.GetRequiredService<ShortestPathView>();
                shortestPathView.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Shortest Path View: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UserSession.Instance.ClearSession();
                this.Close();
                ShowLoginForm();
            }
        }


        private void UpdateProfileButton_Click(object sender, EventArgs e)
        {
            try
            {
                var updateProfileForm = _serviceProvider.GetRequiredService<UpdateProfileForm>();
                if (updateProfileForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the welcome label with the new username
                    LoadUserInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Update Profile form: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowLoginForm()
        {
            var loginForm = _serviceProvider.GetRequiredService<LoginForm>();
            loginForm.Show();
        }

        private void ShowLocationsButton_Click(object sender, EventArgs e)
        {
            var locationsForm = _serviceProvider.GetRequiredService<TravelLocationsForm>();
            locationsForm.Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing && UserSession.Instance.IsLoggedIn())
            {
                var result = MessageBox.Show("Are you sure you want to exit?",
                    "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}