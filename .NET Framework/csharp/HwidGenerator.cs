// File: HwidGenerator.cs
// Purpose: Generates a persistent, unique Hardware ID for the client machine.
// Ensure this namespace matches your .NET application's namespace.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Management; // Requires adding a reference to System.Management.dll to your .NET project

namespace YourWorkspaceName
{
    public static class HwidGenerator
    {
        // Updated to remove nullable reference type syntax for compatibility with C# 7.3  
        private static string _hwid = null;

        // CRITICAL: Change "YourSoftwareName" to a unique name for YOUR application
        // to prevent HWID collisions if multiple apps using this code run on the same machine.
        private static readonly string AppSpecificFolderName = "YourAppName"; /// <<<--- CHANGE THE NAME IF YOU WANT

        // IMPORTANT: Change this to a unique, secret salt for YOUR application.
        // This salt is used to make the generated HWID less predictable and specific to your app.
        // Keep this salt consistent for your application across versions.
        private static readonly string AppSpecificSalt = "YourUniqueAppSalt!@#$%^&*()";

        private static string GetHwidFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Path will be like: C:\Users\YourUser\AppData\Roaming\YourSoftwareName\settings\hwid.dat
            string appPath = Path.Combine(appDataPath, AppSpecificFolderName, "settings");
            Directory.CreateDirectory(appPath); // Ensure directory exists
            return Path.Combine(appPath, "hwid.dat");
        }

        private static string GenerateMachineSpecificId()
        {
            string cpuInfo = string.Empty;
            try
            {
                // Query WMI for the processor ID
                using (ManagementClass mc = new ManagementClass("win32_processor"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        foreach (ManagementObject mo in moc)
                        {
                            if (mo.Properties["processorID"]?.Value != null)
                            {
                                cpuInfo = mo.Properties["processorID"].Value.ToString();
                                break;
                            }
                            mo.Dispose();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Log error if WMI access fails (e.g., permissions issues)
                System.Diagnostics.Debug.WriteLine("Error getting CPU Info for HWID: " + e.Message);
            }

            // Fallback if CPU info isn't available or is empty
            if (string.IsNullOrWhiteSpace(cpuInfo))
            {
                cpuInfo = Environment.MachineName + Environment.UserName + Environment.OSVersion.VersionString;
            }

            // Use a salted SHA256 hash for more privacy and to make it non-obvious
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cpuInfo + AppSpecificSalt));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpperInvariant();
            }
        }

        public static string GetHardwareId()
        {
            if (!string.IsNullOrWhiteSpace(_hwid))
            {
                System.Diagnostics.Debug.WriteLine($"[HWID] Using cached HWID: {_hwid}");
                return _hwid;
            }

            string hwidFilePath = GetHwidFilePath();
            try
            {
                if (File.Exists(hwidFilePath))
                {
                    _hwid = File.ReadAllText(hwidFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(_hwid))
                    {
                        System.Diagnostics.Debug.WriteLine($"[HWID] Loaded HWID from file: {_hwid}");
                        return _hwid;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading HWID file '{hwidFilePath}': {ex.Message}");
            }

            _hwid = GenerateMachineSpecificId();
            System.Diagnostics.Debug.WriteLine($"[HWID] Generated new HWID: {_hwid}");
            try
            {
                File.WriteAllText(hwidFilePath, _hwid);
                System.Diagnostics.Debug.WriteLine($"[HWID] Saved new HWID to file: {_hwid}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing HWID file '{hwidFilePath}': {ex.Message}");
                // If writing fails, we still return the generated HWID for this session,
                // but it won't be persisted for the next run.
            }
            return _hwid;
        }
    }
}
