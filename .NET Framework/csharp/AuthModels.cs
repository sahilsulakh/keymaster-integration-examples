// File: KeyMasterAuthModels.cs
// Purpose: Data structures for KeyMaster API communication.
// Ensure this namespace matches your .NET application's namespace.
namespace YourWorkspaceName
{
    // For License Key Authentication
    public class LicenseAuthRequest
    {
        // This is YOUR KeyMaster Account User ID (Firebase UID).
        // Your .NET application will send this ID to the KeyMaster API.
        public string MasterUserId { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string Hwid { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string Username { get; set; } // Optional end-user username for this activation

        public string Ip { get; set; }
    }

    // For Client App User (Username/Password) Authentication
    // These users are created by you within KeyMaster's "Create Users" section.
    public class ClientUserAuthRequest
    {
        public string MasterUserId { get; set; } = string.Empty; // YOUR KeyMaster Account UID
        public string Username { get; set; } = string.Empty;     // Username of the client app user

        public string Hwid { get; set; } // Hardware ID of the client machine
        public string PasswordPlainText { get; set; } = string.Empty; // Password of the client app user
        public string AppVersion { get; set; } = string.Empty;   // Version of your .NET application
    }

    // Common response structure from KeyMaster API (used for both auth types)
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; }
        public string UserStatus { get; set; } // e.g., "active", "expired", "paused"
    }
}
