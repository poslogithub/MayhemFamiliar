using System;
using System.Net;

namespace MayhemFamiliar
{
    internal class UpdateChecker
    {
        private const string versionUrl = "https://raw.githubusercontent.com/poslogithub/MayhemFamiliar/refs/heads/main/version.txt";
        private const string downloadUrl = "https://github.com/poslogithub/MayhemFamiliar/releases/tag/";
        public static string CheckForUpdate()
        {
            try
            {
                using (var client = new WebClient())
                {
                    string latestVersion = client.DownloadString(versionUrl).Trim();
                    string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    if (latestVersion != currentVersion)
                    {
                        return $"{downloadUrl}{latestVersion}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for update: {ex.Message}");
            }
            return "";
        }
    }
}
