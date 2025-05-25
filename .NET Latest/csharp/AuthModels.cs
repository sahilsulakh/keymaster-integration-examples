namespace YourWorkspaceName // Replace with your application's actual namespace
{
    /// <summary>
    /// Represents the authentication request sent from the .NET application to the KeyMaster API.
    /// </summary>

    public class AuthRequest
    {
        /// Unique ID of the master user (usually the application owner or manager).
        
        public string MasterUserId { get; set; } = string.Empty;

        
        /// License key provided by the end-user of the application.
  
        public string LicenseKey { get; set; } = string.Empty;

        
        /// Hardware ID representing the machine running the application.
        
        public string Hwid { get; set; } = string.Empty;

        
        /// Version of the application (e.g., "1.0.0").
      
        public string AppVersion { get; set; } = string.Empty;

        
        /// Optional: Username of the end-user if the application supports individual user identities.
        
        public string? Username { get; set; }

        
        /// Optional: IP address of the client machine.
       
        public string? Ip { get; set; }
    }

    
    /// Represents the authentication response received from the KeyMaster API.
    
    public class AuthResponse
    {
        
        /// Indicates whether the authentication request was successful.
        
        public bool Success { get; set; }

        
        /// Response message providing additional information or error details.
        
        public string Message { get; set; } = string.Empty;

        
        /// Optional: Error code for failed authentication attempts.
        
        public string? ErrorCode { get; set; }

        
        /// Optional: Status of the user (e.g., active, banned).
        
        public string? UserStatus { get; set; }
    }
}
