// File: KeyMasterAuthModels.cs
// Purpose: Data structures for KeyMaster API communication.
// Compatible with .NET Framework 4.7.2 and latest .NET versions.
using System.Text.Json.Serialization;

namespace YourWorkspaceName
{
    // License Key Authentication Request
    public class LicenseAuthRequest
    {
        public string MasterUserId { get; set; }
        public string LicenseKey { get; set; }
        public string Hwid { get; set; }
        public string AppVersion { get; set; }
        public string Username { get; set; } // Optional
        public string Ip { get; set; } // Optional
    }

    // Client App User Authentication Request
    public class ClientUserAuthRequest
    {
        public string MasterUserId { get; set; }
        public string Username { get; set; }
        public string Hwid { get; set; }
        public string PasswordPlainText { get; set; }
        public string AppVersion { get; set; }
        public string Ip { get; set; } // Optional
    }

    // Common API Response
    public class AuthResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("userStatus")]
        public string UserStatus { get; set; }
    }
}
