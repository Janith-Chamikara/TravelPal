using MongoDB.Driver;

namespace TravelPal.Services;
public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoDbService(string connectionString, string databaseName)
    {
        try 
        {
            _client = new MongoClient(connectionString);
            Console.WriteLine(_client);
            _database = _client.GetDatabase(databaseName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not connect to database: {ex.Message}");
        }
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }
}