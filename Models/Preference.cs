using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelPal.Models
{
    public class Preference
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("label")]
        public string Label { get; set; }
    }
}