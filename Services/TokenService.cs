namespace TravelPal.Services;
public class TokenService
{
    private readonly string _secretKey;

    public TokenService(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string GenerateToken(string userId)
    {
        // Create base token with user ID and timestamp
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        // Combine user ID, timestamp, and secret key
        string tokenBase = $"{userId}:{timestamp}:{_secretKey}";
        
        // Simple hash generation
        int hash = ComputeHash(tokenBase);
        
        // Combine all parts into a token
        return $"{userId}:{timestamp}:{hash}";
    }

    public bool VerifyToken(string token)
    {
        // Split token into parts
        string[] parts = token.Split(':');
        if (parts.Length != 3)
            return false;

        // Parse components
        string userId = parts[0];
        long timestamp = long.Parse(parts[1]);
        int originalHash = int.Parse(parts[2]);

        // Check token expiration (e.g., 1 hour)
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTimestamp - timestamp > 3600)
            return false;

        // Reconstruct token base for verification
        string tokenBase = $"{userId}:{timestamp}:{_secretKey}";
        
        // Compute hash and compare
        int computedHash = ComputeHash(tokenBase);
        
        return computedHash == originalHash;
    }

    private int ComputeHash(string input)
    {
        int hash = 17; // Prime number starting value
        
        foreach (char c in input)
        {
            // Simple hash computation
            hash = hash * 31 + c;
        }
        
        // Ensure positive value
        return Math.Abs(hash);
    }
}