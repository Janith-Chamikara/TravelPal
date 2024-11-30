namespace TravelPal;

using Microsoft.Extensions.DependencyInjection;
using TravelPal.Services;
using TravelPal.UI;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        var services = new ServiceCollection();

        // Register MongoDB service as singleton
        var connectionString = "mongodb+srv://janithchamikara2021:esF6H1ZQ2Q3sYcKJ@cluster0.ci036.mongodb.net/";
        var databaseName = "Cluster0";

        // Register services
        services.AddSingleton(new MongoDbService(connectionString, databaseName));
        services.AddSingleton(new TokenService("your-secret-key-here"));
        services.AddSingleton<AuthService>();
        services.AddTransient<LoginForm>();
        services.AddTransient<SignUpForm>();
        services.AddTransient<DashboardForm>();

        ServiceProvider = services.BuildServiceProvider();

        Application.Run(ServiceProvider.GetRequiredService<LoginForm>());
    }
}