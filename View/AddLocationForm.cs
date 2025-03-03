using MongoDB.Driver;
using System.Diagnostics;
using System.Text.Json;
using TravelPal.Models;
using TravelPal.Services;
using TravelPal.Sessions;

namespace TravelPal.UI
{
    public class AddLocationForm : Form
    {
        private readonly MongoDbService _mongoDbService;
        private TextBox locationNameBox;
        private CheckedListBox preferencesListBox;
        private TextBox searchPreferencesBox;
        private Label coordinatesLabel;
        private RoundedButton addButton;
        private RoundedButton cancelButton;
        private List<Preference> allPreferences;
        private List<string> selectedPreferences;
        private double? latitude;
        private double? longitude;
        private Dictionary<string, string> preferenceIdMap;

        public AddLocationForm(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            selectedPreferences = new List<string>();
            preferenceIdMap = new Dictionary<string, string>();
            InitializeComponents();
            LoadPreferences();
        }

        private void InitializeComponents()
        {
            this.Text = "Add New Location";
            this.Size = new Size(500, 600); // Adjusted size
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Main container
            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Location Input Panel
            Panel locationPanel = new Panel
            {
                Width = 460,
                Height = 100,
                Dock = DockStyle.Top
            };

            Label nameLabel = new Label
            {
                Text = "Location Name:",
                Location = new Point(0, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            locationNameBox = new TextBox
            {
                Location = new Point(0, 35),
                Width = 320,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            RoundedButton checkLocationButton = new RoundedButton
            {
                Text = "Check Location",
                Location = new Point(330, 33),
                Size = new Size(110, 30),
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            checkLocationButton.Click += CheckLocationButton_Click;

            coordinatesLabel = new Label
            {
                Location = new Point(0, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            locationPanel.Controls.AddRange(new Control[] { nameLabel, locationNameBox, checkLocationButton, coordinatesLabel });

            // Preferences Section
            Panel preferencesSection = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 10)
            };

            Label preferencesLabel = new Label
            {
                Text = "Select Preferences:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            searchPreferencesBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search preferences..."
            };
            searchPreferencesBox.TextChanged += SearchPreferencesBox_TextChanged;

            preferencesListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                CheckOnClick = true,
                MultiColumn = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                IntegralHeight = false // Allows for smooth scrolling
            };

            // Buttons Panel
            Panel buttonsPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 10, 0, 0)
            };

            FlowLayoutPanel buttonFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            addButton = new RoundedButton
            {
                Text = "Add Location",
                Size = new Size(120, 35),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Enabled = false,
                Margin = new Padding(0, 0, 10, 0)
            };
            addButton.Click += AddButton_Click;

            cancelButton = new RoundedButton
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderColor = Color.Black, BorderSize = 1 }
            };
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            buttonFlowPanel.Controls.Add(addButton);
            buttonFlowPanel.Controls.Add(cancelButton);
            buttonsPanel.Controls.Add(buttonFlowPanel);

            // Add controls to preferences section
            preferencesSection.Controls.Add(preferencesListBox);
            preferencesSection.Controls.Add(searchPreferencesBox);
            preferencesSection.Controls.Add(preferencesLabel);

            // Add all sections to main container
            mainContainer.Controls.Add(buttonsPanel);
            mainContainer.Controls.Add(preferencesSection);
            mainContainer.Controls.Add(locationPanel);

            this.Controls.Add(mainContainer);
        }

        private async void LoadPreferences()
        {
            try
            {
                var collection = _mongoDbService.GetCollection<Preference>("preferences");
                allPreferences = await collection.Find(_ => true).ToListAsync();
                UpdatePreferencesList(allPreferences);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preferences: {ex.Message}");
            }
        }

        private void UpdatePreferencesList(List<Preference> preferences)
        {
            preferencesListBox.BeginUpdate();
            preferencesListBox.Items.Clear();
            preferenceIdMap.Clear();

            foreach (var pref in preferences)
            {
                preferencesListBox.Items.Add(pref.Label);
                preferenceIdMap[pref.Label] = pref.Id; // Store the mapping between label and Id
            }

            preferencesListBox.EndUpdate();
        }




        private async void CheckLocationButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(locationNameBox.Text))
            {
                MessageBox.Show("Please enter a location name");
                return;
            }

            try
            {
                var coordinates = await GetCoordinatesFromName(locationNameBox.Text);
                if (coordinates != null)
                {
                    latitude = coordinates.Value.lat;
                    longitude = coordinates.Value.lon;
                    coordinatesLabel.Text = $"Coordinates found: {latitude:F6}, {longitude:F6}";
                    addButton.Enabled = true;
                }
                else
                {
                    coordinatesLabel.Text = "Location not found. Please try a more specific name.";
                    addButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking location: {ex.Message}");
            }
        }
        /*
        private void SearchPreferencesBox_TextChanged(object sender, EventArgs e)
        {

            var searchText = searchPreferencesBox.Text.ToLower();
            List<Preference> filteredPreferences = new List<Preference>();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPreferences = allPreferences;
            }
            else
            {
                foreach (var preference in allPreferences)
                {
                    if (preference.Label.ToLower().Contains(searchText))
                    {
                        filteredPreferences.Add(preference);
                    }
                }
            }

            UpdatePreferencesList(filteredPreferences);
        }*/



        //linear search
        //comment
        private void SearchPreferencesBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = searchPreferencesBox.Text.ToLower();
            List<Preference> filteredPreferences = new List<Preference>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPreferences = allPreferences;
            }
            else
            {
                foreach (var preference in allPreferences)
                {
                    if (NaiveStringSearch(preference.Label.ToLower(), searchText))
                    {
                        filteredPreferences.Add(preference);
                    }
                }
            }
            stopwatch.Stop();
            UpdatePreferencesList(filteredPreferences);
        }

        // Naive string search (O(n*m))
        private bool NaiveStringSearch(string text, string pattern)
        {
            int textLength = text.Length;
            int patternLength = pattern.Length;

            for (int i = 0; i <= textLength - patternLength; i++)
            {
                int j;
                for (j = 0; j < patternLength; j++)
                {
                    if (text[i + j] != pattern[j])
                    {
                        break;
                    }
                }
                if (j == patternLength)
                {
                    return true;
                }
            }
            return false;
        }
        
//ens linear search

        private async void AddButton_Click(object sender, EventArgs e)
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                MessageBox.Show("Please check the location first");
                return;
            }

            // Get selected preferences IDs instead of labels
            selectedPreferences.Clear();
            foreach (int index in preferencesListBox.CheckedIndices)
            {
                string label = preferencesListBox.Items[index].ToString();
                if (preferenceIdMap.ContainsKey(label))
                {
                    selectedPreferences.Add(preferenceIdMap[label]); // Add ID instead of label
                }
            }

            if (selectedPreferences.Count == 0)
            {
                MessageBox.Show("Please select at least one preference");
                return;
            }

            try
            {
                var location = new TravelLocation
                {
                    UserId = UserSession.Instance.UserId,
                    LocationName = locationNameBox.Text,
                    Latitude = latitude.Value,
                    Longitude = longitude.Value,
                    Preferences = selectedPreferences // Now contains IDs instead of labels
                };

                var collection = _mongoDbService.GetCollection<TravelLocation>("travelLocations");
                await collection.InsertOneAsync(location);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving location: {ex.Message}");
            }
        }

        private async Task<(double lat, double lon)?> GetCoordinatesFromName(string locationName)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "TravelPal/1.0");
                    var encodedLocation = Uri.EscapeDataString(locationName);
                    var url = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json&limit=1";

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

                    if (results?.Count > 0)
                    {
                        return (double.Parse(results[0].lat, System.Globalization.CultureInfo.InvariantCulture),
                                double.Parse(results[0].lon, System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error getting coordinates: {ex.Message}");
                }
                return null;
            }
        }

        private class NominatimResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
            public string display_name { get; set; }
        }
    }
}