// File: KeyMasterAuthService.cs
// Purpose: Handles HTTP communication with KeyMaster's authentication APIs.
using App1Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json; // For .NET Framework 4.7.2
using System.Text;
using System.Threading.Tasks;

namespace YourWorkspaceName
{
    public class KeyMasterAuthService
    {
        private const string LicenseAuthApiUrl = "https://keymaster-agni.vercel.app/api/authenticate-key";
        private const string ClientUserAuthApiUrl = "https://keymaster-agni.vercel.app/api/authenticate-client-user";
        private static readonly HttpClient client = new HttpClient();

        public KeyMasterAuthService()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<AuthResponse> AuthenticateLicenseAsync(LicenseAuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MasterUserId))
                return new AuthResponse { Success = false, Message = "MasterUserId is not set.", ErrorCode = "CLIENT_MISSING_MASTER_USER_ID" };
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                return new AuthResponse { Success = false, Message = "License Key cannot be empty.", ErrorCode = "CLIENT_MISSING_LICENSE_KEY" };
            if (string.IsNullOrWhiteSpace(request.Hwid))
                return new AuthResponse { Success = false, Message = "HWID is missing.", ErrorCode = "CLIENT_MISSING_HWID" };
            if (string.IsNullOrWhiteSpace(request.AppVersion))
                return new AuthResponse { Success = false, Message = "App Version is missing.", ErrorCode = "CLIENT_MISSING_APP_VERSION" };
            return await SendRequestAsync(LicenseAuthApiUrl, request);
        }

        public async Task<AuthResponse> AuthenticateClientUserAsync(ClientUserAuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MasterUserId))
                return new AuthResponse { Success = false, Message = "MasterUserId is not set.", ErrorCode = "CLIENT_MISSING_MASTER_USER_ID" };
            if (string.IsNullOrWhiteSpace(request.Username))
                return new AuthResponse { Success = false, Message = "Username cannot be empty.", ErrorCode = "CLIENT_MISSING_USERNAME" };
            if (request.PasswordPlainText == null)
                return new AuthResponse { Success = false, Message = "Password cannot be null.", ErrorCode = "CLIENT_MISSING_PASSWORD" };
            if (string.IsNullOrWhiteSpace(request.AppVersion))
                return new AuthResponse { Success = false, Message = "App Version is missing.", ErrorCode = "CLIENT_MISSING_APP_VERSION" };
            return await SendRequestAsync(ClientUserAuthApiUrl, request);
        }

        private async Task<AuthResponse> SendRequestAsync<T>(string apiUrl, T payload)
        {
            try
            {
                string json;
#if NETFRAMEWORK
                using (var ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    serializer.WriteObject(ms, payload);
                    json = Encoding.UTF8.GetString(ms.ToArray());
                }
#else
                json = System.Text.Json.JsonSerializer.Serialize(payload);
#endif
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();
#if NETFRAMEWORK
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(responseBody)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(AuthResponse));
                    return (AuthResponse)serializer.ReadObject(ms);
                }
#else
                return System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(responseBody);
#endif
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Message = "Error: " + ex.Message, ErrorCode = "CLIENT_ERROR" };
            }
        }
    }
}
