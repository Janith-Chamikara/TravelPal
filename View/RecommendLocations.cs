using System.Text.Json;
using TravelPal.Services;
using RestSharp;
using System.Diagnostics;
using TravelPal.DataStructures;
using TravelPal.Sessions;
public class RecommendedLocationsForm : Form
{
    private readonly string FOURSQUARE_API_KEY = "fsq3y/D896CyiVoi0G3szivnvS7i5tgDyY72QFDBdK1Uy9M=";
    private ListView recommendedListView;
    private Label titleLabel;
    private TravelLocation selectedLocation;
    private List<string> preferenceIds;
    private readonly MongoDbService _mongoDbService;
    private readonly CustomGraph _graph;
    private readonly OsmDataLoader _dataLoader;
    private FoursquareResponse apiResults;

    public RecommendedLocationsForm(TravelLocation location, MongoDbService mongoDbService, CustomGraph graph, OsmDataLoader dataLoader)
    {
        selectedLocation = location;
        _mongoDbService = mongoDbService;
        _graph = graph;
        _dataLoader = dataLoader;
        InitializeComponents();
        LoadRecommendedLocations();
    }

    private void InitializeComponents()
    {
        this.Text = "Recommended Locations";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        TableLayoutPanel mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(20)
        };

        titleLabel = new Label
        {
            Text = $"Recommendations near {selectedLocation.LocationName}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 40
        };

        recommendedListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 10)
        };

        recommendedListView.Columns.AddRange(new ColumnHeader[]
        {
            new ColumnHeader { Text = "Name", Width = 200 },
            new ColumnHeader { Text = "Category", Width = 150 },
            new ColumnHeader { Text = "Distance", Width = 100 },
            new ColumnHeader { Text = "Rating", Width = 100 },
            new ColumnHeader { Text = "Address", Width = 200 }
        });

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(recommendedListView);

        this.Controls.Add(mainPanel);
        recommendedListView.DoubleClick += RecommendedListView_DoubleClick;
    }

    private async void LoadRecommendedLocations()
    {
        try
        {
            var baseUrl = $"https://api.foursquare.com/v3/places/search?ll={selectedLocation.Latitude}%2C{selectedLocation.Longitude}";
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("Authorization", FOURSQUARE_API_KEY);

            // Add query parameters
            request.AddQueryParameter("radius", "5000");
            request.AddQueryParameter("limit", "50");
            request.AddQueryParameter("sort", "DISTANCE");

            // Add categories if available
            if (selectedLocation.Preferences?.Any() == true)
            {
                request.AddQueryParameter("categories", string.Join(",", selectedLocation.Preferences));
            }

            var response = await client.ExecuteAsync(request);
            MessageBox.Show($"Response Content: {response.Content}");
            Debug.WriteLine($"Response Content: {request}");
            Debug.WriteLine($"Response Status: {response.StatusCode}");
            Debug.WriteLine($"Response Content: {response.Content}");

            if (response.IsSuccessful)
            {
                apiResults = JsonSerializer.Deserialize<FoursquareResponse>(response.Content); // Store the results

                recommendedListView.Items.Clear();
                if (apiResults?.results != null)
                {
                    foreach (var result in apiResults.results)
                    {
                        var item = new ListViewItem(new[]
                        {
                            result.name,
                            result.categories?.FirstOrDefault()?.name ?? "N/A",
                            $"{result.distance}m",
                            result.rating?.ToString("F1") ?? "N/A",
                            result.location?.formatted_address ?? "N/A"
                        });
                        recommendedListView.Items.Add(item);
                    }
                }
            }
            else
            {
                MessageBox.Show($"API Error: {response.ErrorMessage}\n\nStatus Code: {response.StatusCode}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading recommendations: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void RecommendedListView_DoubleClick(object sender, EventArgs e)
    {
        if (recommendedListView.SelectedItems.Count == 0)
            return;

        try
        {
            var selectedItem = recommendedListView.SelectedItems[0];

            // Add null checks
            if (apiResults?.results == null)
            {
                MessageBox.Show("No results available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = apiResults.results.FirstOrDefault(r => r.name == selectedItem.Text);

            if (result == null)
            {
                MessageBox.Show("Selected location not found in results", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (result.geocodes?.main == null)
            {
                MessageBox.Show("Location coordinates not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if user location is available
            if (UserSession.Instance?.UserLocation == null)
            {
                MessageBox.Show("User location not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var shortestPathView = new ShortestPathView(_graph, _dataLoader);
            var userLocation = UserSession.Instance.UserLocation;

            // Debug information
            Debug.WriteLine($"User Location: {userLocation.Latitude}, {userLocation.Longitude}");
            Debug.WriteLine($"Destination: {result.geocodes.main.latitude}, {result.geocodes.main.longitude}");

            shortestPathView.SetCoordinates(
                userLocation.Latitude,
                userLocation.Longitude,
                result.geocodes.main.latitude,
                result.geocodes.main.longitude
            );

            shortestPathView.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in RecommendedListView_DoubleClick: {ex}");
            MessageBox.Show($"Error showing path: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private class FoursquareResponse
    {
        public List<FoursquareResult> results { get; set; }
    }

    private class FoursquareResult
    {
        public string fsq_id { get; set; }
        public string name { get; set; }
        public List<Category> categories { get; set; }
        public Location location { get; set; }
        public GeoCodes geocodes { get; set; }
        public double? rating { get; set; }
        public int distance { get; set; }
    }

    private class Category
    {
        public int id { get; set; }
        public string name { get; set; }
        public Icon icon { get; set; }
    }

    private class Icon
    {
        public string prefix { get; set; }
        public string suffix { get; set; }
    }

    private class Location
    {
        public string address { get; set; }
        public string formatted_address { get; set; }
        public string country { get; set; }
        public string cross_street { get; set; }
        public string locality { get; set; }
        public string region { get; set; }
        public string postcode { get; set; }
        public string neighborhood { get; set; }
    }

    private class GeoCodes
    {
        public MainPoint main { get; set; }
        public Roof roof { get; set; }
    }

    private class MainPoint
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    private class Roof
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}