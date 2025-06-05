using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceName
{
    public partial class LoginPage : Form
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly KeyMasterAuthService _authService = new KeyMasterAuthService();
        private readonly string _myKeyMasterAccountUID = "cgqtZkiq9tQ47s8EfC7rZbmN9sJ3"; <<<--- REPLACE WITH YOUR MASTER UID FROM KEYMASTER PROFILE

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {

        }

        public static async Task<string> GetPublicIPAsync()
        {
            try
            {
                // You can use other IP services too like "https://checkip.amazonaws.com/"
                string Ip = await httpClient.GetStringAsync("https://api.ipify.org");
                return Ip.Trim(); // Remove any leading/trailing whitespace
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching public IP: {ex.Message}");
                return "Unable to retrieve IP";
            }
        }

        private async void btnAuthenticateWithUserPass_Click(object sender, EventArgs e)
        {
            var request = new ClientUserAuthRequest
            {
                MasterUserId = _myKeyMasterAccountUID,
                Username = txtUsername.Text.Trim(),
                Hwid = HwidGenerator.GetHardwareId(),
                PasswordPlainText = txtPassword.Text,
                AppVersion = Application.ProductVersion
            };
            var response = await _authService.AuthenticateClientUserAsync(request);
            ShowAuthResult(response, "User Login");
        }

        private void ShowAuthResult(AuthResponse response, string authType)
        {
            if (response != null && response.Success)
                MessageBox.Show(authType + " Success! " + response.Message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(authType + " Failed: " + (response != null ? response.Message : "Unknown error"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void btnAuthenticateWithLicenseKey_Click(object sender, EventArgs e)
        {
            var request = new LicenseAuthRequest
            {
                MasterUserId = _myKeyMasterAccountUID,
                LicenseKey = txtLicenseKey.Text.Trim().ToUpper(),
                Ip = await GetPublicIPAsync(),
                Hwid = HwidGenerator.GetHardwareId(),
                AppVersion = Application.ProductVersion,                
            };
            var response = await _authService.AuthenticateLicenseAsync(request);
            ShowAuthResult(response, "License Key");
        }
    }
}

