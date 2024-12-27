using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using TravelPal.DataStructures;
using TravelPal.Services;

public class ShortestPathView : Form
{
    private readonly CustomGraph _graph;
    private readonly OsmDataLoader _dataLoader;
    private TextBox startLatTextBox;
    private TextBox startLonTextBox;
    private TextBox endLatTextBox;
    private TextBox endLonTextBox;
    private Button calculateButton;
    private Label resultLabel;
    private GMapControl gMapControl;

    public ShortestPathView(CustomGraph graph, OsmDataLoader dataLoader)
    {
        _graph = graph;
        _dataLoader = dataLoader;
        InitializeComponents();
        InitializeMap();
    }

    private void InitializeComponents()
    {
        // Make the form larger and start maximized
        this.WindowState = FormWindowState.Maximized;
        this.MinimumSize = new Size(800, 600);

        // Create main container
        TableLayoutPanel mainContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Margin = new Padding(0)
        };

        // Configure row styles
        mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 80f));//nput panel height
        mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Map takes remaining space

        // Input panel
        TableLayoutPanel inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 1,
            Height = 80,
            Margin = new Padding(5, 5, 5, 0)
        };

        // Add input controls with minimal spacing
        inputPanel.Controls.Add(new Label { Text = "Start Lat:", AutoSize = true }, 0, 0);
        startLatTextBox = new TextBox { Width = 80 };
        inputPanel.Controls.Add(startLatTextBox, 1, 0);

        inputPanel.Controls.Add(new Label { Text = "Lon:", AutoSize = true }, 2, 0);
        startLonTextBox = new TextBox { Width = 80 };
        inputPanel.Controls.Add(startLonTextBox, 3, 0);

        inputPanel.Controls.Add(new Label { Text = "End Lat:", AutoSize = true }, 4, 0);
        endLatTextBox = new TextBox { Width = 80 };
        inputPanel.Controls.Add(endLatTextBox, 5, 0);

        inputPanel.Controls.Add(new Label { Text = "Lon:", AutoSize = true }, 6, 0);
        endLonTextBox = new TextBox { Width = 80 };
        inputPanel.Controls.Add(endLonTextBox, 7, 0);

        calculateButton = new Button
        {
            Text = "Find Shortest Path",
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        calculateButton.Click += CalculateButton_Click;
        inputPanel.Controls.Add(calculateButton, 8, 0);

        resultLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(10, 0, 0, 0)
        };
        inputPanel.Controls.Add(resultLabel, 9, 0);

        // Map control
        gMapControl = new GMapControl
        {
            Dock = DockStyle.Fill,
            MapProvider = GMapProviders.OpenStreetMap,
            Position = new PointLatLng(7.8731, 80.7718), // Sri Lanka center
            MaxZoom = 18,
            Zoom = 8,
            ShowCenter = false,
            DragButton = MouseButtons.Left,
            MarkersEnabled = true,
            PolygonsEnabled = true,
            RoutesEnabled = true,
        };

        // Add controls to main container
        mainContainer.Controls.Add(inputPanel, 0, 0);
        mainContainer.Controls.Add(gMapControl, 0, 1);

        this.Controls.Add(mainContainer);

        // Add sample coordinates for Sri Lanka
        startLatTextBox.Text = "6.9271";
        startLonTextBox.Text = "79.8612";
        endLatTextBox.Text = "6.9177";
        endLonTextBox.Text = "79.8583";
    }

    private void InitializeMap()
    {
        GMaps.Instance.Mode = AccessMode.ServerAndCache;
        gMapControl.ShowCenter = false;
        gMapControl.DragButton = MouseButtons.Left;
        gMapControl.MarkersEnabled = true;
        gMapControl.PolygonsEnabled = true;
        gMapControl.RoutesEnabled = true;
    }

    private async void CalculateButton_Click(object sender, EventArgs e)
    {
        try
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Please enter valid coordinates.",
                              "Validation Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return;
            }

            calculateButton.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            resultLabel.Text = "Calculating path...";

            double startLat = double.Parse(startLatTextBox.Text);
            double startLon = double.Parse(startLonTextBox.Text);
            double endLat = double.Parse(endLatTextBox.Text);
            double endLon = double.Parse(endLonTextBox.Text);

            var path = await Task.Run(() => _graph.FindShortestPath(startLat, startLon, endLat, endLon));

            if (path == null || path.Count == 0)
            {
                resultLabel.Text = "No path found between these points.";
                MessageBox.Show("No path found. Try coordinates closer to roads.",
                              "No Path Found",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
                return;
            }

            DisplayPathOnMap(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            resultLabel.Text = "Error finding path.";
        }
        finally
        {
            calculateButton.Enabled = true;
            this.Cursor = Cursors.Default;
        }
    }
    public void SetCoordinates(double startLat, double startLon, double endLat, double endLon)
    {
        startLatTextBox.Text = startLat.ToString();
        startLonTextBox.Text = startLon.ToString();
        endLatTextBox.Text = endLat.ToString();
        endLonTextBox.Text = endLon.ToString();

        // Automatically calculate the path
        CalculateButton_Click(this, EventArgs.Empty);
    }

    private void DisplayPathOnMap(List<Node> path)
    {
        gMapControl.Overlays.Clear();

        // Create overlays
        GMapOverlay markersOverlay = new GMapOverlay("markers");
        GMapOverlay routeOverlay = new GMapOverlay("route");

        // Create route points
        List<PointLatLng> routePoints = path.Select(node =>
            new PointLatLng(node.Latitude, node.Longitude)).ToList();

        // Add markers
        GMarkerGoogle startMarker = new GMarkerGoogle(
            routePoints.First(),
            GMarkerGoogleType.green
        );
        GMarkerGoogle endMarker = new GMarkerGoogle(
            routePoints.Last(),
            GMarkerGoogleType.red
        );

        markersOverlay.Markers.Add(startMarker);
        markersOverlay.Markers.Add(endMarker);

        // Create route
        GMapRoute route = new GMapRoute(routePoints, "Shortest Path")
        {
            Stroke = new Pen(Color.Blue, 3)
        };

        routeOverlay.Routes.Add(route);

        // Add overlays
        gMapControl.Overlays.Add(markersOverlay);
        gMapControl.Overlays.Add(routeOverlay);

        // Zoom to route
        gMapControl.ZoomAndCenterRoute(route);

        // Update result label
        double distance = CalculatePathDistance(path);
        resultLabel.Text = $"Path found! Total distance: {distance:F2} km";
    }

    private double CalculatePathDistance(List<Node> path)
    {
        double totalDistance = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var point1 = path[i];
            var point2 = path[i + 1];
            totalDistance += CalculateDistance(point1.Latitude, point1.Longitude,
                                            point2.Latitude, point2.Longitude);
        }
        return totalDistance;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private bool ValidateInputs()
    {
        return double.TryParse(startLatTextBox.Text, out double startLat) &&
               double.TryParse(startLonTextBox.Text, out double startLon) &&
               double.TryParse(endLatTextBox.Text, out double endLat) &&
               double.TryParse(endLonTextBox.Text, out double endLon) &&
               startLat >= -90 && startLat <= 90 &&
               startLon >= -180 && startLon <= 180 &&
               endLat >= -90 && endLat <= 90 &&
               endLon >= -180 && endLon <= 180;
    }
}