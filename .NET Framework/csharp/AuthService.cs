// File: KeyMasterAuthService.cs
// Purpose: Handles KeyMaster API authentication.
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Configuration; // For config

namespace YourWorkspaceName
{
    public class KeyMasterAuthService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly string _apiBaseUrl;
        public KeyMasterAuthService()
        {
            // Read API URL from config for flexibility
            _apiBaseUrl = ConfigurationManager.AppSettings["KeyMasterApiUrl"] ?? "https://keymaster-agni.vercel.app/api/authenticate-key";
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<AuthResponse> AuthenticateAsync(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MasterUserId))
                return new AuthResponse { Success = false, Message = "KeyMaster UID missing.", ErrorCode = "CLIENT_MISSING_MASTER_USER_ID" };
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                return new AuthResponse { Success = false, Message = "License Key missing.", ErrorCode = "CLIENT_MISSING_LICENSE_KEY" };
            if (string.IsNullOrWhiteSpace(request.Hwid))
                return new AuthResponse { Success = false, Message = "HWID missing.", ErrorCode = "CLIENT_MISSING_HWID" };
            if (string.IsNullOrWhiteSpace(request.AppVersion))
                return new AuthResponse { Success = false, Message = "App version missing.", ErrorCode = "CLIENT_MISSING_APP_VERSION" };
            try
            {
                string jsonRequest = JsonSerializer.Serialize(request, jsonSerializerOptions);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_apiBaseUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<AuthResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<AuthResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.Message))
                        return errorResponse;
                }
                catch { }
                return new AuthResponse { Success = false, Message = $"API Error: {(int)response.StatusCode} - {response.ReasonPhrase}. Response: {responseBody}", ErrorCode = $"HTTP_{(int)response.StatusCode}" };
            }
            catch (HttpRequestException e)
            {
                return new AuthResponse { Success = false, Message = "Network error: " + e.Message, ErrorCode = "NETWORK_ERROR" };
            }
            catch (JsonException e)
            {
                return new AuthResponse { Success = false, Message = "JSON error: " + e.Message, ErrorCode = "JSON_PARSE_ERROR" };
            }
            catch (TaskCanceledException e)
            {
                return new AuthResponse { Success = false, Message = "Timeout: " + e.Message, ErrorCode = "TIMEOUT_ERROR" };
            }
            catch (Exception e)
            {
                return new AuthResponse { Success = false, Message = "Unexpected error: " + e.Message, ErrorCode = "UNEXPECTED_CLIENT_ERROR" };
            }
        }
    }
}