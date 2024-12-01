using Microsoft.Extensions.DependencyInjection; 
using TravelPal.Services;

namespace TravelPal.UI
{
    public partial class SignUpForm : Form
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;
        public SignUpForm(AuthService authService, TokenService tokenService)
        {
            _authService = authService ;
            _tokenService = tokenService ;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Sign Up";
            this.ClientSize = new Size(400, 400); // Adjusted for more space
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(50, 50, 50); // Dark background for a modern look

            // Gradient Background
            var gradientPanel = new Panel() { Dock = DockStyle.Fill };
            var color1 = Color.FromArgb(40, 40, 40);
            var color2 = Color.FromArgb(70, 70, 70);
            using (Graphics g = gradientPanel.CreateGraphics())
            {
                var rect = gradientPanel.ClientRectangle;
                var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, color1, color2, 90F);
                g.FillRectangle(brush, rect);
            }

            // Full Name Label
            var emailLabel = new Label()
            {
                Text = "Email",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                Location = new Point(50, 50),
                AutoSize = true
            };

            // Full Name TextBox
            var emailTextBox = new TextBox()
            {
                Location = new Point(50, 80),
                Width = 300,
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                BackColor = Color.White
            };

            // Username Label
            var usernameLabel = new Label()
            {
                Text = "Username",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                Location = new Point(50, 120),
                AutoSize = true
            };

            // Username TextBox
            var usernameTextBox = new TextBox()
            {
                Location = new Point(50, 150),
                Width = 300,
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                BackColor = Color.White
            };

            // Password Label
            var passwordLabel = new Label()
            {
                Text = "Password",
                ForeColor = Color.White,
                Font = new Font("Arial", 12),
                Location = new Point(50, 180),
                AutoSize = true
            };

            // Password TextBox
            var passwordTextBox = new TextBox()
            {
                Location = new Point(50, 210),
                Width = 300,
                Font = new Font("Arial", 10),
                ForeColor = Color.Black,
                BackColor = Color.White,
                PasswordChar = '●'
            };

            // Sign Up Button
            var signUpButton = new Button()
            {
                Text = "Sign Up",
                Location = new Point(50, 250),
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 120, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            signUpButton.FlatAppearance.BorderSize = 0;
            signUpButton.Click += (sender, e) => SignUpButton_Click(sender, e, usernameTextBox.Text, emailTextBox.Text, passwordTextBox.Text);


            // Login Link
            var loginLink = new LinkLabel()
            {
                Text = "Already have an account? Login here",
                ForeColor = Color.White,
                Font = new Font("Arial", 10),
                Location = new Point(50, 300),
                AutoSize = true
            };
            loginLink.LinkColor = Color.White;  // Set the color of the link text
            loginLink.VisitedLinkColor = Color.White; // Color after the link is clicked
            loginLink.ActiveLinkColor = Color.Gray; 
            loginLink.LinkClicked += (sender, e) => LoginLink_Click(sender, e);

            // Add Controls to Form
            gradientPanel.Controls.Add(emailLabel);
            gradientPanel.Controls.Add(emailTextBox);
            gradientPanel.Controls.Add(usernameLabel);
            gradientPanel.Controls.Add(usernameTextBox);
            gradientPanel.Controls.Add(passwordLabel);
            gradientPanel.Controls.Add(passwordTextBox);
            gradientPanel.Controls.Add(signUpButton);
            gradientPanel.Controls.Add(loginLink);
            this.Controls.Add(gradientPanel);
        }

        private async void SignUpButton_Click(object sender, EventArgs e, string username, string fullName, string password)
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

                if (string.IsNullOrWhiteSpace(fullName))
                {
                     MessageBox.Show("Email is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                     MessageBox.Show("Password is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                     return;
                }
                // Attempt to login
                var user = await _authService.RegisterAsync(username,fullName, password);

                // Generate token for the user
                var token = _tokenService.GenerateToken(user.Id);

                MessageBox.Show("Sign Up Successful!", "Success", 
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

        private void LoginLink_Click(object sender, EventArgs e)
        {
            // Hide the current form (Sign Up)
            this.Hide();

            // Open the Login form
            var loginForm = Program.ServiceProvider.GetRequiredService<LoginForm>();
            loginForm.Show();
        }
    }
}
