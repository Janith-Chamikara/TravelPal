using MongoDB.Driver;
using TravelPal.Models;
using TravelPal.Services;
using TravelPal.Sessions;
using TravelPal.UI;

public class UpdateProfileForm : Form
{
    private readonly MongoDbService _mongoDbService;
    private TextBox usernameTextBox;
    private TextBox latitudeTextBox;
    private TextBox longitudeTextBox;
    private Label currentLocationLabel;
    private RoundedButton updateButton;
    private RoundedButton cancelButton;

    public UpdateProfileForm(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
        InitializeComponents();
        LoadUserData();
    }

    private void InitializeComponents()
    {
        this.Text = "Update Profile";
        this.Size = new Size(400, 380); // Increased height for username field
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        TableLayoutPanel mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5, // Increased row count
            Padding = new Padding(20),
            BackColor = Color.White
        };

        // Username Input
        mainPanel.Controls.Add(new Label
        {
            Text = "Username:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Black,
            AutoSize = true
        }, 0, 0);

        usernameTextBox = new TextBox
        {
            Width = 150,
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.FixedSingle
        };
        mainPanel.Controls.Add(usernameTextBox, 1, 0);

        // Current Location Label
        currentLocationLabel = new Label
        {
            Text = "Current Location: Not Set",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Black,
            AutoSize = true,
            Margin = new Padding(0, 15, 0, 15)
        };
        mainPanel.Controls.Add(currentLocationLabel, 0, 1);
        mainPanel.SetColumnSpan(currentLocationLabel, 2);

        // Latitude Input
        mainPanel.Controls.Add(new Label
        {
            Text = "Latitude:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Black,
            AutoSize = true
        }, 0, 2);

        latitudeTextBox = new TextBox
        {
            Width = 150,
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.FixedSingle
        };
        mainPanel.Controls.Add(latitudeTextBox, 1, 2);

        // Longitude Input
        mainPanel.Controls.Add(new Label
        {
            Text = "Longitude:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Black,
            AutoSize = true
        }, 0, 3);

        longitudeTextBox = new TextBox
        {
            Width = 150,
            Font = new Font("Segoe UI", 10),
            BorderStyle = BorderStyle.FixedSingle
        };
        mainPanel.Controls.Add(longitudeTextBox, 1, 3);

        // Buttons Panel
        FlowLayoutPanel buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 0)
        };

        updateButton = new RoundedButton
        {
            Text = "Update Profile",
            BackColor = Color.Black,
            ForeColor = Color.White,
            Size = new Size(120, 35),
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        updateButton.Click += UpdateButton_Click;

        cancelButton = new RoundedButton
        {
            Text = "Cancel",
            BackColor = Color.White,
            ForeColor = Color.Black,
            Size = new Size(100, 35),
            Font = new Font("Segoe UI", 10),
            Margin = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        cancelButton.Click += (s, e) => this.Close();

        buttonPanel.Controls.Add(updateButton);
        buttonPanel.Controls.Add(cancelButton);
        mainPanel.Controls.Add(buttonPanel, 0, 4);
        mainPanel.SetColumnSpan(buttonPanel, 2);

        this.Controls.Add(mainPanel);
    }

    private async void LoadUserData()
    {
        try
        {
            var collection = _mongoDbService.GetCollection<User>("users");
            var user = await collection.Find(u => u.Id == UserSession.Instance.UserId)
                                    .FirstOrDefaultAsync();

            if (user != null)
            {
                usernameTextBox.Text = user.Username;

                if (user.UserLocation != null)
                {
                    currentLocationLabel.Text = $"Current Location: {user.UserLocation.Latitude}, {user.UserLocation.Longitude}";
                    latitudeTextBox.Text = user.UserLocation.Latitude.ToString();
                    longitudeTextBox.Text = user.UserLocation.Longitude.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading user data: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void UpdateButton_Click(object sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            MessageBox.Show("Please ensure:\n- Username is not empty\n- Valid coordinates\n  Latitude: -90 to 90\n  Longitude: -180 to 180",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            updateButton.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            var collection = _mongoDbService.GetCollection<User>("users");

            // Check if username is already taken
            if (await IsUsernameTaken(usernameTextBox.Text))
            {
                MessageBox.Show("This username is already taken.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var location = new Location
            {
                Latitude = double.Parse(latitudeTextBox.Text),
                Longitude = double.Parse(longitudeTextBox.Text)
            };

            var update = Builders<User>.Update
                .Set(u => u.Username, usernameTextBox.Text)
                .Set(u => u.UserLocation, location);

            var result = await collection.UpdateOneAsync(
                u => u.Id == UserSession.Instance.UserId,
                update
            );

            if (result.ModifiedCount > 0)
            {
                // Update session username
                UserSession.Instance.Username = usernameTextBox.Text;
                UserSession.Instance.UserLocation = location;

                MessageBox.Show("Profile updated successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("No changes were made.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating profile: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            updateButton.Enabled = true;
            this.Cursor = Cursors.Default;
        }
    }

    private async Task<bool> IsUsernameTaken(string username)
    {
        var collection = _mongoDbService.GetCollection<User>("users");
        var existingUser = await collection.Find(u =>
            u.Username == username &&
            u.Id != UserSession.Instance.UserId
        ).FirstOrDefaultAsync();

        return existingUser != null;
    }

    private bool ValidateInputs()
    {
        return !string.IsNullOrWhiteSpace(usernameTextBox.Text) &&
               double.TryParse(latitudeTextBox.Text, out double lat) &&
               double.TryParse(longitudeTextBox.Text, out double lon) &&
               lat >= -90 && lat <= 90 &&
               lon >= -180 && lon <= 180;
    }
}