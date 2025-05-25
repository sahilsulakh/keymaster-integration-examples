using System;
using System.IO;

namespace YourWorkspaceName
{
    public static class HwidGenerator
    {
        private static string _hwid = null;

        private static readonly string HwidFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "App1", // <<<--- CHANGE THIS TO YOUR SOFTWARE'S NAME
            "settings",
            "hwid.dat"
        );

        public static string GetHardwareId()
        {
            if (_hwid != null) return _hwid;

            try
            {
                string directoryPath = Path.GetDirectoryName(HwidFilePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
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
                System.Diagnostics.Debug.WriteLine($"Error reading HWID file: {ex.Message}");
            }

            // Generate and persist new HWID
            _hwid = Guid.NewGuid().ToString().ToUpper();
            try
            {
                File.WriteAllText(HwidFilePath, _hwid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing HWID file: {ex.Message}");
            }

            return _hwid;
        }
    }
}
