using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceName
{
    public partial class LoginPage : Form
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly KeyMasterAuthService _authService;
        private const string KeyMasterUidPlaceholder = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE"; // DO NOT TOUCH ANYTHING HERE
        private string _myKeyMasterAccountUID = "YOUR KEYMASTER UID FROM KEYMASTER PROFILE";   // <<<--- REPLACE WITH YOUR KEYMASTER UID FROM KEYMASTER PROFILE

        public LoginPage()
        {
            InitializeComponent();

            _authService = new KeyMasterAuthService();

            // Client-side check: Ensure YOU have configured your MasterUserId in this .NET app's code.
            if (string.IsNullOrWhiteSpace(_myKeyMasterAccountUID) ||
                _myKeyMasterAccountUID == KeyMasterUidPlaceholder)
            {
                MessageBox.Show(
                    "CRITICAL CONFIGURATION ERROR: The KeyMaster Account UID for this application's developer has not been set in the application's code. Please open LoginForm.cs (or similar) and replace 'PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE' with your actual UID from your KeyMaster Profile page.",
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                // Consider disabling login buttons if this ID is essential
                if (btnAuthenticateWithLicenseKey != null) btnAuthenticateWithLicenseKey.Enabled = false;
                if (btnAuthenticateWithUserPass != null) btnAuthenticateWithUserPass.Enabled = false;
            }
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {
            // You can load any initial data here if needed
        }

        private bool IsMasterUidNotSet()
        {
            if (string.IsNullOrWhiteSpace(_myKeyMasterAccountUID) || _myKeyMasterAccountUID == KeyMasterUidPlaceholder)
            {
                MessageBox.Show(
                   "Application is not configured correctly to connect to KeyMaster. " +
                   "The developer's KeyMaster Account UID is missing or still set to the placeholder. " +
                   "Please replace the placeholder in the C# code (LoginForm.cs or similar) " +
                   "with your actual UID from your KeyMaster Profile page.",
                   "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the public IP address of the user.
        /// </summary>
        public static async Task<string> GetPublicIpAsync()
        {
            try
            {
                
                return await _httpClient.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Handles login button click and authenticates user.
        /// </summary>
        private async void btnActivate_Click_1(object sender, EventArgs e)
        {
            if (IsMasterUidNotSet()) return;

            btnAuthenticateWithLicenseKey.Enabled = false;
            if (lblStatus != null) lblStatus.Text = "Authenticating License Key...";

            var request = new LicenseAuthRequest
            {
                MasterUserId = _myKeyMasterAccountUID,
                LicenseKey = txtLicenseKey.Text.Trim().ToUpper(), // Ensure txtLicenseKey.Text is from your UI
                Hwid = HwidGenerator.GetHardwareId(),
                AppVersion = Application.ProductVersion, // Gets your .NET app's version
                //Username = string.IsNullOrWhiteSpace(txtLicenseUsername.Text) ? null : txtLicenseUsername.Text.Trim() // Optional
            };

            AuthResponse response = await _authService.AuthenticateLicenseAsync(request);
            HandleAuthResponse(response, "License Key");
            btnAuthenticateWithLicenseKey.Enabled = true;
        }


        private async void btnAuthenticateWithUserPass_Click(object sender, EventArgs e)
        {
            if (IsMasterUidNotSet()) return;

            btnAuthenticateWithUserPass.Enabled = false;
            if (lblStatus != null) lblStatus.Text = "Authenticating Application User...";

            var request = new ClientUserAuthRequest
            {
                MasterUserId = _myKeyMasterAccountUID,
                Username = txtUsername.Text.Trim(), // Ensure txtClientAppUsername.Text is from your UI
                Hwid = HwidGenerator.GetHardwareId(),
                PasswordPlainText = txtPassword.Text, // Ensure txtClientAppPassword.Text is from your UI
                AppVersion = Application.ProductVersion
            };

            AuthResponse response = await _authService.AuthenticateClientUserAsync(request);
            HandleAuthResponse(response, "Application User");
            btnAuthenticateWithUserPass.Enabled = true;
        }

        // --- Common method to handle the API response ---
        private void HandleAuthResponse(AuthResponse response, string authType)
        {
            if (response != null && response.Success)
            {
                MessageBox.Show(
                    $"Type: {authType}\nMessage: {response.Message}\nUser Status: {response.UserStatus ?? "N/A"}",
                    "Authentication Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                // TODO: Unlock your application's main features here.
                // Example: 
                // this.Hide(); 
                // YourMainApplicationForm mainApp = new YourMainApplicationForm(); 
                // mainApp.ShowDialog(); 
                // this.Close();
            }
            else
            {
                string errorMessage = response?.Message ?? $"An unknown {authType.ToLower()} authentication error occurred.";
                string errorCode = response?.ErrorCode ?? "N/A";
                MessageBox.Show(
                    $"Type: {authType}\nMessage: {errorMessage}\nError Code: {errorCode}\nUser Status: {response?.UserStatus ?? "N/A"}",
                    "Authentication Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            if (lblStatus != null) lblStatus.Text = "Ready.";
        }
    }
}

