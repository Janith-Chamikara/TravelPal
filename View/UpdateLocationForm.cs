using MongoDB.Driver;
using System.Text.Json;
using TravelPal.Models;
using TravelPal.Services;
using TravelPal.Sessions;

namespace TravelPal.UI
{
    public class UpdateLocationForm : Form
    {
        private readonly MongoDbService _mongoDbService;
        private readonly string _originalName;
        private readonly TravelLocation _originalLocation;
        private TextBox locationNameBox;
        private CheckedListBox preferencesListBox;
        private TextBox searchPreferencesBox;
        private Label coordinatesLabel;
        private RoundedButton updateButton;
        private RoundedButton cancelButton;
        private List<Preference> allPreferences;
        private List<string> selectedPreferences;
        private double? latitude;
        private double? longitude;
        private Dictionary<string, string> preferenceIdMap;

        public UpdateLocationForm(MongoDbService mongoDbService, TravelLocation location)
        {
            _mongoDbService = mongoDbService;
            _originalLocation = location;
            _originalName = location.LocationName;
            selectedPreferences = new List<string>(location.Preferences);
            preferenceIdMap = new Dictionary<string, string>();
            latitude = location.Latitude;
            longitude = location.Longitude;

            InitializeComponents();
            LoadPreferences();
        }

        private void InitializeComponents()
        {
            this.Text = "Update Location";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 5
            };

            // Location Name Section
            var locationPanel = new Panel { Dock = DockStyle.Top, Height = 80 };
            locationPanel.Controls.Add(new Label
            {
                Text = "Location Name:",
                Location = new Point(0, 10),
                AutoSize = true
            });

            locationNameBox = new TextBox
            {
                Text = _originalLocation.LocationName,
                Location = new Point(0, 35),
                Width = 320
            };
            locationPanel.Controls.Add(locationNameBox);

            // Coordinates Section
            coordinatesLabel = new Label
            {
                Text = $"Coordinates: {_originalLocation.Latitude:F6}, {_originalLocation.Longitude:F6}",
                Location = new Point(0, 65),
                AutoSize = true
            };
            locationPanel.Controls.Add(coordinatesLabel);

            // Preferences Search Section
            var preferencesPanel = new Panel { Dock = DockStyle.Fill };
            preferencesPanel.Controls.Add(new Label
            {
                Text = "Search Preferences:",
                Location = new Point(0, 0),
                AutoSize = true
            });

            searchPreferencesBox = new TextBox
            {
                Location = new Point(0, 25),
                Width = 320
            };
            searchPreferencesBox.TextChanged += SearchPreferencesBox_TextChanged;
            preferencesPanel.Controls.Add(searchPreferencesBox);

            // Preferences List
            preferencesListBox = new CheckedListBox
            {
                Location = new Point(0, 60),
                Size = new Size(320, 200),
                CheckOnClick = true
            };
            preferencesPanel.Controls.Add(preferencesListBox);

            // Buttons Panel
            var buttonsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            updateButton = new RoundedButton
            {
                Text = "Update Location",
                Size = new Size(120, 35),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Margin = new Padding(10, 0, 0, 0)
            };
            updateButton.Click += UpdateButton_Click;

            cancelButton = new RoundedButton
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonsPanel.Controls.AddRange(new Control[] { cancelButton, updateButton });

            // Add all panels to main layout
            mainLayout.Controls.Add(locationPanel);
            mainLayout.Controls.Add(preferencesPanel);
            mainLayout.Controls.Add(buttonsPanel);

            this.Controls.Add(mainLayout);
        }

        private async void LoadPreferences()
        {
            try
            {
                var collection = _mongoDbService.GetCollection<Preference>("preferences");
                allPreferences = await collection.Find(_ => true).ToListAsync();
                UpdatePreferencesList(allPreferences);

                // Check previously selected preferences
                foreach (var pref in selectedPreferences)
                {
                    int index = preferencesListBox.Items.IndexOf(pref);
                    if (index != -1)
                    {
                        preferencesListBox.SetItemChecked(index, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preferences: {ex.Message}");
            }
        }

        private void UpdatePreferencesList(List<Preference> preferences)
        {
            preferencesListBox.Items.Clear();
            preferenceIdMap.Clear();

            foreach (var pref in preferences)
            {
                preferencesListBox.Items.Add(pref.Id);
                preferenceIdMap[pref.Id] = pref.Id;
            }
        }

        private void SearchPreferencesBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = searchPreferencesBox.Text.ToLower();
            var filteredPreferences = allPreferences
                .Where(p => p.Label.ToLower().Contains(searchText))
                .ToList();
            UpdatePreferencesList(filteredPreferences);
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(locationNameBox.Text))
                {
                    MessageBox.Show("Location name is required.");
                    return;
                }

                var collection = _mongoDbService.GetCollection<TravelLocation>("travelLocations");
                var filter = Builders<TravelLocation>.Filter.And(
                    Builders<TravelLocation>.Filter.Eq(l => l.UserId, UserSession.Instance.UserId),
                    Builders<TravelLocation>.Filter.Eq(l => l.LocationName, _originalName)
                );

                var selectedPrefs = new List<string>();
                foreach (int index in preferencesListBox.CheckedIndices)
                {
                    string label = preferencesListBox.Items[index].ToString();
                    if (preferenceIdMap.ContainsKey(label))
                    {
                        selectedPrefs.Add(preferenceIdMap[label]);
                    }
                }

                var update = Builders<TravelLocation>.Update
                    .Set(l => l.LocationName, locationNameBox.Text)
                    .Set(l => l.Latitude, latitude.Value)
                    .Set(l => l.Longitude, longitude.Value)
                    .Set(l => l.Preferences, selectedPrefs);

                await collection.UpdateOneAsync(filter, update);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating location: {ex.Message}");
            }
        }
    }
}