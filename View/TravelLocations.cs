using System.Text.Json;
using MongoDB.Driver;
using TravelPal.DataStructures;
using TravelPal.Services;
using TravelPal.Sessions;

namespace TravelPal.UI
{
    public class TravelLocationsForm : Form
    {
        private readonly MongoDbService _mongoDbService;
        private readonly CustomGraph _graph;
        private readonly OsmDataLoader _dataLoader;
        private readonly TravelLocationList locationList;
        private TextBox searchBox;
        private TextBox locationNameBox;
        private ListView locationListView;
        private RoundedButton addButton;
        private RoundedButton removeButton;
        private RoundedButton searchButton;

        public TravelLocationsForm(MongoDbService mongoDbService, CustomGraph graph, OsmDataLoader dataLoader)
        {
            _mongoDbService = mongoDbService;
            locationList = new TravelLocationList();
            _graph = graph;
            _dataLoader = dataLoader;
            InitializeComponents();
            LoadUserLocations();
        }

        private void InitializeComponents()
        {
            this.Text = "My Travel Locations";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(20)
            };

            // Search Panel
            Panel searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            searchBox = new TextBox
            {
                Width = 200,
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            searchButton = new RoundedButton
            {
                Text = "Search",
                Size = new Size(80, 30),
                Location = new Point(220, 8),
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            searchButton.Click += SearchButton_Click;

            searchPanel.Controls.AddRange(new Control[] { searchBox, searchButton });

            // Add Location Panel
            Panel addPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            locationNameBox = new TextBox
            {
                Width = 200,
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter location name"
            };

            addButton = new RoundedButton
            {
                Text = "Add",
                Size = new Size(80, 30),
                Location = new Point(220, 8),
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            addButton.Click += AddButton_Click;

            removeButton = new RoundedButton
            {
                Text = "Remove",
                Size = new Size(80, 30),
                Location = new Point(310, 8),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            removeButton.Click += RemoveButton_Click;

            addPanel.Controls.AddRange(new Control[] { locationNameBox, addButton, removeButton });

            // Locations ListView
            locationListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10)
            };
            locationListView.Columns.AddRange(new ColumnHeader[]
            {
            new ColumnHeader { Text = "Location Name", Width = 200 },
            new ColumnHeader { Text = "Latitude", Width = 100 },
            new ColumnHeader { Text = "Longitude", Width = 100 },
            new ColumnHeader { Text = "View more",Width = 1}
            });

            mainPanel.Controls.Add(searchPanel);
            mainPanel.Controls.Add(addPanel);
            mainPanel.Controls.Add(locationListView);

            this.Controls.Add(mainPanel);
            locationListView.DoubleClick += LocationListView_DoubleClick;
        }

        private async void LoadUserLocations()
        {
            try
            {
                locationList.Clear(); // Clear the list before loading
                var collection = _mongoDbService.GetCollection<TravelLocation>("travelLocations");
                var locations = await collection.Find(l => l.UserId == UserSession.Instance.UserId).ToListAsync();

                foreach (var location in locations)
                {
                    locationList.AddLocation(location.LocationName, location.Latitude, location.Longitude);
                }

                RefreshLocationsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading locations: {ex.Message}");
            }
        }
        private void RefreshLocationsList()
        {
            locationListView.Items.Clear();
            var locations = locationList.GetAllLocations();

            foreach (var location in locations)
            {
                var item = new ListViewItem(new[]
                {
                location.LocationName,
                location.Latitude.ToString("F6"),
                location.Longitude.ToString("F6")
            });
                locationListView.Items.Add(item);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var addLocationForm = new AddLocationForm(_mongoDbService);
            if (addLocationForm.ShowDialog() == DialogResult.OK)
            {
                locationList.Clear(); // Clear the list before reloading
                LoadUserLocations(); // Refresh the list
            }
        }
        private async void RemoveButton_Click(object sender, EventArgs e)
        {
            if (locationListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a location to remove");
                return;
            }

            var locationName = locationListView.SelectedItems[0].Text;
            if (locationList.RemoveLocation(locationName))
            {
                // Remove from MongoDB
                var collection = _mongoDbService.GetCollection<TravelLocation>("travelLocations");
                await collection.DeleteOneAsync(l =>
                    l.UserId == UserSession.Instance.UserId &&
                    l.LocationName == locationName);

                RefreshLocationsList();
            }
        }
        private async void LocationListView_DoubleClick(object sender, EventArgs e)
        {
            if (locationListView.SelectedItems.Count == 0)
                return;

            var selectedItem = locationListView.SelectedItems[0];
            var locationName = selectedItem.Text;

            try
            {
                var collection = _mongoDbService.GetCollection<TravelLocation>("travelLocations");
                var location = await collection.Find(l =>
                    l.UserId == UserSession.Instance.UserId &&
                    l.LocationName == locationName)
                    .FirstOrDefaultAsync();

                if (location != null)
                {
                    var recommendedForm = new RecommendedLocationsForm(location, _mongoDbService, _graph, _dataLoader);
                    recommendedForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading location details: {ex.Message}");
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                RefreshLocationsList();
                return;
            }

            var location = locationList.SearchLocation(searchBox.Text);
            if (location != null)
            {
                locationListView.Items.Clear();
                locationListView.Items.Add(new ListViewItem(new[]
                {
                location.LocationName,
                location.Latitude.ToString("F6"),
                location.Longitude.ToString("F6")
            }));
            }
            else
            {
                MessageBox.Show("Location not found");
            }
        }

        private async Task<(double lat, double lon)?> GetCoordinatesFromName(string locationName)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Add User-Agent header as required by Nominatim
                    client.DefaultRequestHeaders.Add("User-Agent", "TravelPal/1.0");

                    // Properly encode the location name
                    var encodedLocation = Uri.EscapeDataString(locationName);
                    var url = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json&limit=1";

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {content}"); // Debug line

                    var results = JsonSerializer.Deserialize<List<NominatimResult>>(content);

                    if (results != null && results.Count > 0)
                    {
                        Console.WriteLine($"Found coordinates: {results[0].lat}, {results[0].lon}"); // Debug line
                        return (double.Parse(results[0].lat, System.Globalization.CultureInfo.InvariantCulture),
                                double.Parse(results[0].lon, System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting coordinates: {ex.Message}");
                    Console.WriteLine($"Error: {ex.Message}"); // Debug line
                }
                return null;
            }
        }

        // Update the NominatimResult class
        private class NominatimResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
            public string display_name { get; set; } // Added for more information
        }
    }
}