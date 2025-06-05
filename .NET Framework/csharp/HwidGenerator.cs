// File: HwidGenerator.cs
// Purpose: Generates a persistent, unique Hardware ID for the client machine.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Management; // Add System.Management to your project references

namespace YourWorkspaceName
{
    public static class HwidGenerator
    {
        private static string _hwid;
        private static readonly string AppSpecificFolderName = "YourAppName"; // Change to your app name
        private static readonly string AppSpecificSalt = "YourUniqueAppSalt!@#$%^&*()"; // Change to your app's salt

        private static string GetHwidFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appPath = Path.Combine(appDataPath, AppSpecificFolderName, "settings");
            if (!Directory.Exists(appPath)) Directory.CreateDirectory(appPath);
            return Path.Combine(appPath, "hwid.dat");
        }

        private static string GenerateMachineSpecificId()
        {
            string cpuInfo = string.Empty;
            try
            {
                using (var mc = new ManagementClass("win32_processor"))
                using (var moc = mc.GetInstances())
                {
                    foreach (ManagementObject mo in moc)
                    {
                        if (mo["processorID"] != null)
                        {
                            cpuInfo = mo["processorID"].ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                cpuInfo = Environment.MachineName + Environment.UserName + Environment.OSVersion.VersionString;
            }
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(cpuInfo + AppSpecificSalt));
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        public static string GetHardwareId()
        {
            if (!string.IsNullOrWhiteSpace(_hwid)) return _hwid;
            string path = GetHwidFilePath();
            try
            {
                if (File.Exists(path))
                {
                    _hwid = File.ReadAllText(path).Trim();
                    if (!string.IsNullOrWhiteSpace(_hwid)) return _hwid;
                }
            }
            catch { }
            _hwid = GenerateMachineSpecificId();
            try { File.WriteAllText(path, _hwid); } catch { }
            return _hwid;
        }
    }
}
