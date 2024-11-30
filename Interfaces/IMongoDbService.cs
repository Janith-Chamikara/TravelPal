using MongoDB.Driver;

namespace TravelPal.Services
{
    public interface IMongoDbService
    {
        // Get a MongoDB collection of type T
        IMongoCollection<T> GetCollection<T>(string collectionName);

        // Get the MongoDB database instance
        IMongoDatabase GetDatabase();
    }
}