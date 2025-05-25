namespace YourWorkspaceName
{
    public class AuthRequest
    {
        public string MasterUserId { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string Hwid { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;

        // Removed nullable syntax
        public string Username { get; set; }
        public string Ip { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; }
        public string UserStatus { get; set; }
    }
}
