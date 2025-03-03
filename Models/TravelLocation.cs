using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class TravelLocation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string LocationName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<string> Preferences { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }


    public TravelLocation()
    {
        CreatedAt = DateTime.UtcNow;
    }
}