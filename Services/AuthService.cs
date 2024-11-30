using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using TravelPal.Models;

namespace TravelPal.Services
{
    public class AuthService
    {
        private readonly MongoDbService _mongoDb;
        private readonly IMongoCollection<User> _users;

        public AuthService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
            _users = _mongoDb.GetCollection<User>("users");
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            // Check if user already exists
            var existingUser = await _users.Find(u => 
                u.Username == username || u.Email == email).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                throw new Exception("Username or email already exists");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username)
                                 .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                throw new Exception("Invalid password");
            }

            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}