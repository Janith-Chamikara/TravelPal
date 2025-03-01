namespace TravelPal;

using Microsoft.Extensions.DependencyInjection;
using TravelPal.Algorithms;
using TravelPal.DataStructures;
using TravelPal.Services;
using TravelPal.UI;
using System.Windows.Forms;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;


    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        // Register MongoDB service as singleton
        var connectionString = "mongodb+srv://janithchamikara2021:6ggVGzrWK3zCUlwc@cluster0.ci036.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        var databaseName = "Cluster0";

        // Create and configure graph with OSM data
        var graph = new CustomGraph();
        // var osmLoader = new OsmDataLoader(graph);

        // // Load OSM data at startup
        // try
        // {
        //     Console.WriteLine("Loading OSM data...");
        //     string projectBaseDirectory = Directory.GetCurrentDirectory(); // Gets the root directory of the project
        //     string osmFilePath = Path.Combine(projectBaseDirectory, "Assets", "sri-lanka-latest.osm.pbf");


        //     if (!File.Exists(osmFilePath))
        //     {
        //         MessageBox.Show($"OSM file not found at: {osmFilePath}",
        //                       "File Not Found",
        //                       MessageBoxButtons.OK,
        //                       MessageBoxIcon.Error);
        //         return;
        //     }

        //     osmLoader.LoadOsmData(osmFilePath);
        //     Console.WriteLine($"OSM data loaded successfully! Total nodes: {graph.NodeCount()}");

        //     if (graph.NodeCount() == 0)
        //     {
        //         MessageBox.Show("No nodes were loaded from the OSM file. Please check the data.",
        //                       "Warning",
        //                       MessageBoxButtons.OK,
        //                       MessageBoxIcon.Warning);
        //     }
        //     else
        //     {
        //         Console.WriteLine($"Successfully loaded {graph.NodeCount()} nodes");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Error loading OSM data: {ex.Message}");
        //     MessageBox.Show($"Error loading OSM data: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
        //                   "Error",
        //                   MessageBoxButtons.OK,
        //                   MessageBoxIcon.Error);
        // }

        // Register services
        services.AddSingleton(new MongoDbService(connectionString, databaseName));
        services.AddSingleton(new TokenService("your-secret-key-here"));
        services.AddSingleton<AuthService>();

        // Register graph services
        services.AddSingleton(graph);  // Register the pre-loaded graph
        // services.AddSingleton(osmLoader);

        // Register forms
        services.AddTransient<LoginForm>();
        services.AddTransient<SignUpForm>();
        services.AddTransient<DashboardForm>();
        services.AddTransient<UpdateProfileForm>();
        services.AddTransient<ShortestPathView>();
        services.AddTransient<TravelLocationsForm>();
        services.AddTransient<AddLocationForm>();


        ServiceProvider = services.BuildServiceProvider();

        // Add error handling for the main application
        Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        try
        {
            Application.Run(ServiceProvider.GetRequiredService<LoginForm>());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Critical Error: {ex.Message}\n\nThe application needs to close.",
                          "Critical Error",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
        }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        MessageBox.Show($"Thread Error: {e.Exception.Message}\n\nStack Trace: {e.Exception.StackTrace}",
                      "Error",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Error);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"Unhandled Error: {ex?.Message}\n\nStack Trace: {ex?.StackTrace}",
                      "Critical Error",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Error);
    }
}