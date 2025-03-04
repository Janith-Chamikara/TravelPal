using MongoDB.Driver;
using System.Diagnostics;
using System.Text.Json;
using TravelPal.Models;
using TravelPal.Services;
using TravelPal.Sessions;
using TravelPal.Algorithms;
using System.Diagnostics;

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

                //SortAlgorithms.BubbleSort(allPreferences,
                //                  (p1, p2) => string.Compare(p1.Label ?? "", p2.Label ?? ""));

                SortAlgorithms.QuickSort(allPreferences, 0, allPreferences.Count - 1,
                                        (p1, p2) => string.Compare(p1.Label ?? "", p2.Label ?? ""));

                //SortAlgorithms.MergeSort(allPreferences,
                //                (p1, p2) => string.Compare(p1.Label ?? "", p2.Label ?? ""));

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

        /*

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
                // Perform a linear search over all preferences
                foreach (var preference in allPreferences)
                {
                    // Check if the label matches the search text manually (without inbuilt methods)
                    if (LinearSearch(preference.Label.ToLower(), searchText))
                    {
                        filteredPreferences.Add(preference);
                    }
                }
            }

            stopwatch.Stop();
            UpdatePreferencesList(filteredPreferences);
        }

        // Custom linear search (no inbuilt methods)
        private bool LinearSearch(string text, string pattern)
        {
            int textLength = text.Length;
            int patternLength = pattern.Length;

            // Loop through the text
            for (int i = 0; i <= textLength - patternLength; i++)
            {
                bool match = true;

                // Compare each character of the pattern with the corresponding character in the text
                for (int j = 0; j < patternLength; j++)
                {
                    if (text[i + j] != pattern[j])
                    {
                        match = false;
                        break; // No match, exit the inner loop
                    }
                }

                if (match)
                {
                    return true; // Pattern found
                }
            }

            return false; // Pattern not found
        }

        */
        //end linear search

        /*
                //start kmp algorithm

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
                            if (KMP_Search(preference.Label.ToLower(), searchText))
                            {
                                filteredPreferences.Add(preference);
                            }
                        }
                    }
                    stopwatch.Stop();   
                    UpdatePreferencesList(filteredPreferences);
                }

                // KMP algorithm (O(n + m))
                private bool KMP_Search(string text, string pattern)
                {
                    int[] lps = ComputeLPSArray(pattern);
                    int i = 0, j = 0;

                    while (i < text.Length)
                    {
                        if (pattern[j] == text[i])
                        {
                            i++; j++;
                        }
                        if (j == pattern.Length)
                        {
                            return true;
                        }
                        else if (i < text.Length && pattern[j] != text[i])
                        {
                            if (j != 0)
                            {
                                j = lps[j - 1];
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }
                    return false;
                }

                // Compute the longest prefix suffix (LPS) array
                private int[] ComputeLPSArray(string pattern)
                {
                    int[] lps = new int[pattern.Length];
                    int length = 0;
                    int i = 1;

                    while (i < pattern.Length)
                    {
                        if (pattern[i] == pattern[length])
                        {
                            length++;
                            lps[i] = length;
                            i++;
                        }
                        else
                        {
                            if (length != 0)
                            {
                                length = lps[length - 1];
                            }
                            else
                            {
                                lps[i] = 0;
                                i++;
                            }
                        }
                    }
                    return lps;
                }

        //End KMP algorithm
        */

        /*
        //start jump search
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
                // Assuming allPreferences is sorted alphabetically by Label
                int index = JumpSearch(allPreferences, searchText);
                if (index >= 0)
                {
                    filteredPreferences.Add(allPreferences[index]);
                }
            }
            stopwatch.Stop();
            UpdatePreferencesList(filteredPreferences);
        }

        // Jump Search (O(√n))
        private int JumpSearch(List<Preference> preferences, string searchText)
        {


            int n = preferences.Count;
            int step = (int)Math.Sqrt(n);
            int prev = 0;

            while (preferences[Math.Min(step, n) - 1].Label.ToLower().CompareTo(searchText) < 0)
            {
                prev = step;
                step += (int)Math.Sqrt(n);
                if (prev >= n) return -1;
            }

            for (int i = prev; i < Math.Min(step, n); i++)
            {
                if (preferences[i].Label.ToLower().Contains(searchText))
                {
                    return i; // Found
                }
            }
            return -1;  // Not found
        }
        */
        //end jump search
        /*
        //rabin karp search
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
                    if (RabinKarpSearch(preference.Label.ToLower(), searchText))
                    {
                        filteredPreferences.Add(preference);
                    }
                }
            }
            stopwatch.Stop();
            UpdatePreferencesList(filteredPreferences);
        }
        
        // Rabin-Karp string search
        private bool RabinKarpSearch(string text, string pattern)
        {
            int prime = 101; // A prime number for hashing
            int m = pattern.Length;
            int n = text.Length;
            int patternHash = 0, textHash = 0, h = 1;

            for (int i = 0; i < m - 1; i++)
                h = (h * 256) % prime;

            for (int i = 0; i < m; i++)
            {
                patternHash = (256 * patternHash + pattern[i]) % prime;
                textHash = (256 * textHash + text[i]) % prime;
            }

            for (int i = 0; i <= n - m; i++)
            {
                if (patternHash == textHash)
                {
                    int j;
                    for (j = 0; j < m; j++)
                        if (text[i + j] != pattern[j])
                            break;

                    if (j == m)
                        return true;
                }

                if (i < n - m)
                {
                    textHash = (256 * (textHash - text[i] * h) + text[i + m]) % prime;
                    if (textHash < 0)
                        textHash += prime;
                }
            }
            return false;
        }
//end rabin karp search
        */

        
        //Boyer Moore Algorithm

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
                    if (BoyerMooreSearch(preference.Label.ToLower(), searchText))
                    {
                        filteredPreferences.Add(preference);
                    }
                }
            }
            stopwatch.Stop();
            UpdatePreferencesList(filteredPreferences);
        }

        // Boyer-Moore algorithm
        private bool BoyerMooreSearch(string text, string pattern)
        {
            int[] badCharTable = BuildBadCharTable(pattern);
            int m = pattern.Length;
            int n = text.Length;
            int shift = 0;

            while (shift <= (n - m))
            {
                int j = m - 1;
                while (j >= 0 && pattern[j] == text[shift + j])
                    j--;

                if (j < 0)
                {
                    return true;
                }
                else
                {
                    shift += Math.Max(1, j - badCharTable[text[shift + j]]);
                }
            }
            return false;
        }

        private int[] BuildBadCharTable(string pattern)
        {
            int[] table = new int[256];
            for (int i = 0; i < 256; i++)
                table[i] = -1;
            for (int i = 0; i < pattern.Length; i++)
                table[pattern[i]] = i;
            return table;
        }
        

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