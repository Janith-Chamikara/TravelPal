using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelPal.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRequired]
        public required string Username { get; set; }

        [BsonRequired]
        public required string Email { get; set; }

        [BsonRequired]
        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}