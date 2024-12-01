using TravelPal.Services;
using Microsoft.Extensions.DependencyInjection; 

namespace TravelPal.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;

        public LoginForm(AuthService authService, TokenService tokenService)
        {
           _authService = authService;
           _tokenService = tokenService;
           InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Login";
            this.ClientSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(50, 50, 50);

            var gradientPanel = new Panel() { Dock = DockStyle.Fill };
            var color1 = Color.FromArgb(40, 40, 40);
            var color2 = Color.FromArgb(70, 70, 70);
            
            // Use a more reliable way to create gradient background
            gradientPanel.Paint += (sender, e) =>
            {
                var rect = gradientPanel.ClientRectangle;
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, color1, color2, 90F);
                e.Graphics.FillRectangle(brush, rect);
            };

            var usernameLabel = new Label()
            {
                Text = "Username",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                Location = new Point(50, 50),
                AutoSize = true
            };

            var usernameTextBox = new TextBox()
            {
                Location = new Point(50, 80),
                Width = 300,
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                BackColor = Color.White
            };

            var passwordLabel = new Label()
            {
                Text = "Password",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                Location = new Point(50, 120),
                AutoSize = true
            };

            var passwordTextBox = new TextBox()
            {
                Location = new Point(50, 150),
                Width = 300,
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                BackColor = Color.White,
                PasswordChar = '●'
            };

            var loginButton = new Button()
            {
                Text = "Login",
                Location = new Point(50, 190),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 120, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += (sender, e) => LoginButton_Click(sender!, e, usernameTextBox.Text, passwordTextBox.Text);

            var signUpLink = new LinkLabel()
            {
                Text = "Don't have an account yet? Sign up here",
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                Location = new Point(50, 240),
                AutoSize = true
            };
            signUpLink.LinkColor = Color.White;
            signUpLink.VisitedLinkColor = Color.White;
            signUpLink.ActiveLinkColor = Color.Gray; 
            signUpLink.LinkClicked += (sender, e) => SignUpLink_Click(sender!, e);

            // Add Controls to Form
            gradientPanel.Controls.Add(usernameLabel);
            gradientPanel.Controls.Add(usernameTextBox);
            gradientPanel.Controls.Add(passwordLabel);
            gradientPanel.Controls.Add(passwordTextBox);
            gradientPanel.Controls.Add(loginButton);
            gradientPanel.Controls.Add(signUpLink);
            this.Controls.Add(gradientPanel);
        }

        private async void LoginButton_Click(object? sender, EventArgs e, string username, string password)
        {
            try
            {
                // Show loading indicator
                Cursor = Cursors.WaitCursor;

                if (string.IsNullOrWhiteSpace(username))
                {
                     MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                     MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     return;
                }

                // Attempt to login
                var user = await _authService.LoginAsync(username, password);

                // Generate token for the user
                var token = _tokenService.GenerateToken(user.Id ?? throw new InvalidOperationException("User ID is null"));

                MessageBox.Show("Login Successful!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Hide the login form
                this.Hide();

                // Open dashboard with the authenticated user
                var dashboard = Program.ServiceProvider.GetRequiredService<DashboardForm>();
                dashboard.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Login Failed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset cursor
                Cursor = Cursors.Default;
            }
        }

        private void SignUpLink_Click(object? sender, EventArgs e)
        {
            this.Hide();
            var signUpForm = Program.ServiceProvider.GetRequiredService<SignUpForm>();
            signUpForm.Show();
        }
    }
}