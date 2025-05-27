// File: KeyMasterAuthService.cs
// Purpose: Handles HTTP communication with KeyMaster's authentication APIs.
// Ensure this namespace matches your .NET application's namespace.
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json; // Namespace for System.Text.Json
using System.Text.Json.Serialization; // For JsonIgnoreCondition
using System.Threading.Tasks;
using System.Diagnostics; // For Debug.WriteLine

namespace YourWorkspaceName
{
    public class KeyMasterAuthService
    {
        // CRITICAL: This is the base URL for YOUR deployed KeyMaster application's API.
        // Replace 'keymaster-agni.vercel.app' with your actual domain if different.
        private const string LicenseAuthApiUrl = "https://keymaster-agni.vercel.app/api/authenticate-key";
        //private const string LicenseAuthApiUrl = "http://localhost:9002/api/authenticate-key";
        private const string ClientUserAuthApiUrl = "https://keymaster-agni.vercel.app/api/authenticate-client-user";
        //private const string ClientUserAuthApiUrl = "http://localhost:9002/api/authenticate-client-user";

        private static readonly HttpClient client = new HttpClient();

        // Configure JsonSerializer to send JSON property names in camelCase (standard for web APIs)
        // and to ignore null values when serializing.
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public KeyMasterAuthService()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(30); // Set a reasonable timeout
        }

        // --- Method for License Key Authentication ---
        public async Task<AuthResponse> AuthenticateLicenseAsync(LicenseAuthRequest request)
        {
            // Basic client-side validation
            if (string.IsNullOrWhiteSpace(request.MasterUserId) || request.MasterUserId == "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE") // DO NOT TOUCH ANYTHING HERE
                return new AuthResponse { Success = false, Message = "Configuration Error: MasterUserId is not set in the .NET app.", ErrorCode = "CLIENT_MISSING_MASTER_USER_ID" };
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                return new AuthResponse { Success = false, Message = "License Key cannot be empty.", ErrorCode = "CLIENT_MISSING_LICENSE_KEY" };
            if (string.IsNullOrWhiteSpace(request.Hwid))
                return new AuthResponse { Success = false, Message = "Hardware ID (HWID) is missing.", ErrorCode = "CLIENT_MISSING_HWID" };
            if (string.IsNullOrWhiteSpace(request.AppVersion))
                return new AuthResponse { Success = false, Message = "Application Version is missing.", ErrorCode = "CLIENT_MISSING_APP_VERSION" };

            return await SendRequestAsync<LicenseAuthRequest, AuthResponse>(LicenseAuthApiUrl, request);
        }

        // --- Method for Client App User (Username/Password) Authentication ---
        public async Task<AuthResponse> AuthenticateClientUserAsync(ClientUserAuthRequest request)
        {
            // Basic client-side validation
            if (string.IsNullOrWhiteSpace(request.MasterUserId) || request.MasterUserId == "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE") // DO NOT TOUCH ANYTHING HERE
                return new AuthResponse { Success = false, Message = "Configuration Error: MasterUserId is not set in the .NET app.", ErrorCode = "CLIENT_MISSING_MASTER_USER_ID" };
            if (string.IsNullOrWhiteSpace(request.Username))
                return new AuthResponse { Success = false, Message = "Username cannot be empty.", ErrorCode = "CLIENT_MISSING_USERNAME" };
            if (request.PasswordPlainText == null) // Password can be empty string if allowed by backend, but should not be null for auth attempt
                return new AuthResponse { Success = false, Message = "Password cannot be null.", ErrorCode = "CLIENT_MISSING_PASSWORD" };
            if (string.IsNullOrWhiteSpace(request.AppVersion))
                return new AuthResponse { Success = false, Message = "Application Version is missing.", ErrorCode = "CLIENT_MISSING_APP_VERSION" };

            return await SendRequestAsync<ClientUserAuthRequest, AuthResponse>(ClientUserAuthApiUrl, request);
        }

        // --- Generic helper method to send requests ---
        private async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string apiUrl, TRequest requestPayload)
            where TResponse : class // Ensure TResponse is a reference type for null return
        {
            try
            {
                string jsonRequest = JsonSerializer.Serialize(requestPayload, jsonSerializerOptions);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                Debug.WriteLine($"Sending JSON to {apiUrl}: {jsonRequest}");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"KeyMaster API Response Status from {apiUrl}: {response.StatusCode}");
                Debug.WriteLine($"KeyMaster API Response Body from {apiUrl}: {responseBody}");

                // Try to deserialize into TResponse regardless of status code, as error responses might conform to it.
                TResponse result = null;
                try
                {
                    result = JsonSerializer.Deserialize<TResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException deserEx)
                {
                    Debug.WriteLine($"Could not deserialize response body into {typeof(TResponse).Name}: {deserEx.Message}. Body: {responseBody}");
                }

                // If deserialization succeeded and it's an error response from KeyMaster (Success=false but has Message)
                if (result != null && typeof(TResponse) == typeof(AuthResponse))
                {
                    var authResp = result as AuthResponse;
                    if (authResp != null && !authResp.Success && !string.IsNullOrEmpty(authResp.Message))
                    {
                        return result; // Return the parsed error response from KeyMaster
                    }
                }

                // If it's not a success status code and we couldn't parse a KeyMaster error response
                if (!response.IsSuccessStatusCode && (result == null || (result as AuthResponse)?.Success != false))
                {
                    var genericError = Activator.CreateInstance<TResponse>() as AuthResponse;
                    if (genericError != null)
                    {
                        genericError.Success = false;
                        genericError.Message = $"API Error: {(int)response.StatusCode} - {response.ReasonPhrase}. Response: {responseBody.Substring(0, Math.Min(responseBody.Length, 500))}"; // Limit response body in message
                        genericError.ErrorCode = $"HTTP_{(int)response.StatusCode}";
                    }
                    return genericError as TResponse;
                }

                return result; // Return successful deserialized response or parsed KeyMaster error
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"HttpRequestException in SendRequestAsync for {apiUrl}: {e.Message}");
                var errorResp = Activator.CreateInstance<TResponse>() as AuthResponse;
                if (errorResp != null) { errorResp.Success = false; errorResp.Message = "Network error: Could not connect to KeyMaster. " + e.Message; errorResp.ErrorCode = "NETWORK_ERROR"; }
                return errorResp as TResponse;
            }
            catch (JsonException e) // Should be caught by the inner try-catch for deserialization mostly
            {
                Debug.WriteLine($"JsonException in SendRequestAsync for {apiUrl} (serialization or unexpected): {e.Message}");
                var errorResp = Activator.CreateInstance<TResponse>() as AuthResponse;
                if (errorResp != null) { errorResp.Success = false; errorResp.Message = "JSON processing error: " + e.Message; errorResp.ErrorCode = "JSON_PROCESS_ERROR"; }
                return errorResp as TResponse;
            }
            catch (TaskCanceledException e) // Catches timeouts
            {
                Debug.WriteLine($"TaskCanceledException (Timeout) in SendRequestAsync for {apiUrl}: {e.Message}");
                var errorResp = Activator.CreateInstance<TResponse>() as AuthResponse;
                if (errorResp != null) { errorResp.Success = false; errorResp.Message = "Request to KeyMaster timed out. " + e.Message; errorResp.ErrorCode = "TIMEOUT_ERROR"; }
                return errorResp as TResponse;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Generic Exception in SendRequestAsync for {apiUrl}: {e.Message}");
                var errorResp = Activator.CreateInstance<TResponse>() as AuthResponse;
                if (errorResp != null) { errorResp.Success = false; errorResp.Message = "An unexpected client error occurred: " + e.Message; errorResp.ErrorCode = "UNEXPECTED_CLIENT_ERROR"; }
                return errorResp as TResponse;
            }
        }
    }
}
