using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using TravelPal.DataStructures;
using TravelPal.Services;

public class ShortestPathView : Form
{
    private readonly CustomGraph _graph;
    // private readonly OsmDataLoader _dataLoader;
    private TextBox startLatTextBox;
    private TextBox startLonTextBox;
    private TextBox endLatTextBox;
    private TextBox endLonTextBox;
    private Button calculateButton;
    private Label resultLabel;
    private GMapControl gMapControl;

    public ShortestPathView(CustomGraph graph)
    {
        _graph = graph;
        // _dataLoader = dataLoader;
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

            // var path = await Task.Run(() => _graph.FindShortestPath(startLat, startLon, endLat, endLon));

            // if (path == null || path.Count == 0)
            // {
            //     resultLabel.Text = "No path found between these points.";
            //     MessageBox.Show("No path found. Try coordinates closer to roads.",
            //                   "No Path Found",
            //                   MessageBoxButtons.OK,
            //                   MessageBoxIcon.Information);
            //     return;
            // }

            PlotLocations(startLat, startLon, endLat, endLon);

            // DisplayPathOnMap(path);
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

    private void InitializeComponent()
    {

    }

    private bool IsWithinSriLanka(double lat, double lon)
    {
        // Sri Lanka's approximate boundaries
        const double SL_MIN_LAT = 5.916667;  // Southernmost point
        const double SL_MAX_LAT = 9.850000;  // Northernmost point
        const double SL_MIN_LON = 79.683333; // Westernmost point
        const double SL_MAX_LON = 81.883333; // Easternmost point

        return lat >= SL_MIN_LAT && lat <= SL_MAX_LAT &&
               lon >= SL_MIN_LON && lon <= SL_MAX_LON;
    }

    private bool ValidateInputs()
    {
        if (!double.TryParse(startLatTextBox.Text, out double startLat) ||
            !double.TryParse(startLonTextBox.Text, out double startLon) ||
            !double.TryParse(endLatTextBox.Text, out double endLat) ||
            !double.TryParse(endLonTextBox.Text, out double endLon))
        {
            MessageBox.Show("Please enter valid coordinates.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!IsWithinSriLanka(startLat, startLon) || !IsWithinSriLanka(endLat, endLon))
        {
            MessageBox.Show("Both locations must be within Sri Lanka's boundaries.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    public void PlotLocations(double location1Lat, double location1Lon, double location2Lat, double location2Lon)
    {
        try
        {
            // Validate coordinates are within Sri Lanka
            if (!IsWithinSriLanka(location1Lat, location1Lon) || !IsWithinSriLanka(location2Lat, location2Lon))
            {
                MessageBox.Show("Both locations must be within Sri Lanka's boundaries.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            gMapControl.Overlays.Clear();

            // Create overlays
            GMapOverlay markersOverlay = new GMapOverlay("markers");
            GMapOverlay routeOverlay = new GMapOverlay("route");

            // Create markers for both locations
            GMarkerGoogle location1Marker = new GMarkerGoogle(
                new PointLatLng(location1Lat, location1Lon),
                GMarkerGoogleType.green
            );
            GMarkerGoogle location2Marker = new GMarkerGoogle(
                new PointLatLng(location2Lat, location2Lon),
                GMarkerGoogleType.red
            );

            // Add markers to overlay
            markersOverlay.Markers.Add(location1Marker);
            markersOverlay.Markers.Add(location2Marker);

            // Create route points
            List<PointLatLng> routePoints = new List<PointLatLng>
            {
                new PointLatLng(location1Lat, location1Lon),
                new PointLatLng(location2Lat, location2Lon)
            };

            // Create route
            GMapRoute route = new GMapRoute(routePoints, "Direct Path")
            {
                Stroke = new Pen(Color.Blue, 3)
            };

            routeOverlay.Routes.Add(route);

            // Add overlays
            gMapControl.Overlays.Add(markersOverlay);
            gMapControl.Overlays.Add(routeOverlay);

            // Set initial view to Sri Lanka
            double centerLat = (location1Lat + location2Lat) / 2;
            double centerLon = (location1Lon + location2Lon) / 2;
            gMapControl.Position = new PointLatLng(centerLat, centerLon);

            // Calculate appropriate zoom level based on distance between points
            double distance = CalculateDistance(location1Lat, location1Lon, location2Lat, location2Lon);
            int zoomLevel = CalculateZoomLevel(distance);
            // Ensure zoom level shows Sri Lanka context
            zoomLevel = Math.Min(zoomLevel, 12); // Cap maximum zoom level
            gMapControl.Zoom = zoomLevel;

            // Update result label with distance
            resultLabel.Text = $"Distance between points: {distance:F2} km";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error plotting locations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private int CalculateZoomLevel(double distanceInKm)
    {
        // Simple algorithm to determine zoom level based on distance
        if (distanceInKm > 1000) return 5;
        if (distanceInKm > 500) return 6;
        if (distanceInKm > 200) return 7;
        if (distanceInKm > 100) return 8;
        if (distanceInKm > 50) return 9;
        if (distanceInKm > 20) return 10;
        if (distanceInKm > 10) return 11;
        if (distanceInKm > 5) return 12;
        if (distanceInKm > 2) return 13;
        if (distanceInKm > 1) return 14;
        return 15;
    }
}