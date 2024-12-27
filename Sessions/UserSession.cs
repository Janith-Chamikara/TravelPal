using TravelPal.Models;
namespace TravelPal.Sessions
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Location UserLocation { get; set; }
        // Add any other user properties you want to maintain

        private static UserSession _instance;
        private static readonly object _lock = new object();

        private UserSession() { }

        public static UserSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserSession();
                        }
                    }
                }
                return _instance;
            }
        }

        public void SetUserSession(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Email = user.Email;
            UserLocation = user.UserLocation;
        }

        public void ClearSession()
        {
            UserId = null;
            Username = null;
            Email = null;
            UserLocation = null;
        }

        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(UserId);
        }
    }
}