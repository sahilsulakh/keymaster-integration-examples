using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceName // Replace with your actual namespace
{
    public partial class LoginPage : Form
    {
        private readonly KeyMasterAuthService _authService;

        // Replace with your actual KeyMaster UID
        private readonly string _myKeyMasterAccountUID = "YOUR_UID_FROM_KEYMASTER_PROFILE"; <<<--- REPLACE THE PLACEHOLDER WITH YOUR KEYMASTER UID

        public LoginPage()
        {
            InitializeComponent();

            _authService = new KeyMasterAuthService();
            CheckAppStatus(); // Optional: implement as needed

            if (string.IsNullOrWhiteSpace(_myKeyMasterAccountUID) || _myKeyMasterAccountUID == "YOUR_KEYMASTER_UID_HERE") // <<<--- ⚠️ DO NOT CHANGE ANYTHING HERE
            {
                MessageBox.Show("KeyMaster UID not set. Please set your UID in LoginPage.cs.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = false;
            }
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {
            // Add any startup logic here
        }

        
        /// Retrieves the user's public IP address using a simple web API.
        
        public static async Task<string?> GetPublicIpAsync()
        {
            try
            {
                using (var http = new HttpClient())
                {
                    return await http.GetStringAsync("https://api.ipify.org");
                }
            }
            catch
            {
                return null;
            }
        }

        
        /// Handles the login button click event and authenticates the user.
        
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblStatus.Text = "Authenticating...";
            lblStatus.ForeColor = Color.DarkOrange;

            string? ip = await GetPublicIpAsync();

            try
            {
                var authRequest = new AuthRequest
                {
                    MasterUserId = _myKeyMasterAccountUID,
                    LicenseKey = txtLicenseKey.Text.Trim().ToUpper(),
                    Hwid = HwidGenerator.GetHardwareId(),
                    AppVersion = Application.ProductVersion,
                    Ip = ip
                };

                var authResult = await _authService.AuthenticateAsync(authRequest);

                if (authResult != null && authResult.Success)
                {
                    lblStatus.Text = "Authentication successful!";
                    lblStatus.ForeColor = Color.Green;

                    MessageBox.Show(
                        $"{authResult.Message}\nStatus: {authResult.UserStatus ?? "N/A"}",
                        "Authenticated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // TODO: Redirect to dashboard or main form
                }
                else
                {
                    lblStatus.Text = authResult?.Message ?? "Authentication error.";
                    lblStatus.ForeColor = Color.Red;

                    MessageBox.Show(
                        $"{authResult?.Message ?? "Unknown error."}\nError Code: {authResult?.ErrorCode ?? "N/A"}",
                        "Authentication Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "An unexpected error occurred.";
                lblStatus.ForeColor = Color.Red;

                MessageBox.Show(
                    $"An error occurred:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                await Task.Delay(700);
                btnLogin.Enabled = true;
                lblStatus.Text = string.Empty;
            }
        }        
    }
}
