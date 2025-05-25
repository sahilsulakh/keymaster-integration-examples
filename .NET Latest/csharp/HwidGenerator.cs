using System;
using System.IO;
// using System.Security.Cryptography; // For more advanced HWID (not used in this simple GUID version)
// using System.Text; // Also for more advanced HWID

namespace YourWorkspaceName // Replace with your application's actual namespace
{
    public static class HwidGenerator
    {
        private static string? _hwid = null;

        // Path to the file where the HWID will be stored
        // Make sure to replace "App1" with your actual application's name

        private static readonly string HwidFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "App1", // Change this to match your software's folder name
            "settings",
            "hwid.dat"
        );


        /// Retrieves a unique hardware identifier (HWID) for the current machine.
        /// If an existing HWID is found, it is reused; otherwise, a new one is generated and saved.

        /// <returns>Hardware ID as a string</returns>

        public static string GetHardwareId()
        {
            if (_hwid != null) return _hwid;

            try
            {
                string? directoryPath = Path.GetDirectoryName(HwidFilePath);
                if (directoryPath != null && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (File.Exists(HwidFilePath))
                {
                    _hwid = File.ReadAllText(HwidFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(_hwid)) return _hwid;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read HWID file: {ex.Message}");
            }

            // Generate a new HWID if no valid one is found

            _hwid = Guid.NewGuid().ToString().ToUpper();
            try
            {
                File.WriteAllText(HwidFilePath, _hwid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write HWID file: {ex.Message}");
            }

            return _hwid;
        }
    }
}
