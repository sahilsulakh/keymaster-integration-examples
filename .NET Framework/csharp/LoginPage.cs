// Example: WinForms LoginPage (LoginPage.cs)
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YourWorkspaceName
{
    public partial class LoginPage : Form
    {
        private KeyMasterAuthService _authService;
        private string _myKeyMasterAccountUID = "YOUR_UID_FROM_KEYMASTER_PROFILE"; // <<<--- REPLACE THIS PLACEHOLDER WITH YOUR ACTUAL KEYMASTER UID!

        public LoginPage()
        {
            InitializeComponent();
            _authService = new KeyMasterAuthService();

            _authService = new KeyMasterAuthService();
            if (string.IsNullOrWhiteSpace(_myKeyMasterAccountUID) || _myKeyMasterAccountUID == "YOUR_KEYMASTER_UID_HERE") // <<<--- ⚠️ DO NOT TOUCH ANYTHING HERE
            {
                MessageBox.Show("KeyMaster UID not set. Please set your UID in LoginForm.cs.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = false;
            }
        }

        private void LoginPage_Load(object sender, EventArgs e)
        {
        }

        public static async Task<string> GetPublicIpAsync()
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

        private async void btnLogin_Click_1(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblStatus.Text = "Authenticating...";
            lblStatus.ForeColor = Color.DarkOrange;

            var ip = await GetPublicIpAsync();

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
                        "Authenticated", MessageBoxButtons.OK, MessageBoxIcon.Information
                    );
                }
                else
                {
                    lblStatus.Text = authResult?.Message ?? "Authentication error.";
                    lblStatus.ForeColor = Color.Red;

                    MessageBox.Show(
                        $"{authResult?.Message ?? "Unknown error."}\nError Code: {authResult?.ErrorCode ?? "N/A"}",
                        "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "An unexpected error occurred.";
                lblStatus.ForeColor = Color.Red;

                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                await Task.Delay(700);
                btnLogin.Enabled = true;
                lblStatus.Text = "";
            }
        }
    }
}