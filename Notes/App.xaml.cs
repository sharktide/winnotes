using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Notes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Call the async update checking logic when the app starts
            CheckForUpdatesAsync();

            MainPage = new AppShell();
        }

        // Async method to check for updates
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                string updateUrl = "https://sharkapps.vercel.app/apps/windows/download/winnotes/latest"; // URL to your update XML
                var httpClient = new HttpClient();

                // Download the XML from the server
                var response = await httpClient.GetStringAsync(updateUrl);
                var latestXml = XDocument.Parse(response);

                // Extract the current version (this would be from your app's current version)
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string updateVersion = latestXml.Descendants("UpdateVersion").FirstOrDefault()?.Value;

                if (updateVersion != null && IsUpdateAvailable(currentVersion, updateVersion))
                {
                    string updatePackageUrl = latestXml.Descendants("PackageUrl").FirstOrDefault()?.Value;
                    string releaseNotes = latestXml.Descendants("ReleaseNotes").FirstOrDefault()?.Value;

                    // Output extracted values for debugging purposes
                    Console.WriteLine("Update Available!");
                    Console.WriteLine($"Update URL: {updatePackageUrl}");
                    Console.WriteLine($"Release Notes: {releaseNotes}");

                    // You can now call your download and install logic
                    await DownloadAndInstallUpdateAsync(updatePackageUrl);
                }
                else
                {
                    Console.WriteLine("No update available.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors, XML parsing errors, etc.)
                Console.WriteLine("Error checking for updates: " + ex.Message);
            }
        }

        // Helper to check if an update is available
        private bool IsUpdateAvailable(string currentVersion, string updateVersion)
        {
            Version current = new Version(currentVersion);
            Version update = new Version(updateVersion);
            return update > current;
        }

        // Async method to download and install the update MSIX
        private async Task DownloadAndInstallUpdateAsync(string updateUrl)
        {
            try
            {
                var httpClient = new HttpClient();

                // Download the MSIX installer to a temporary file
                var msixData = await httpClient.GetByteArrayAsync(updateUrl);
                string tempPath = Path.Combine(Path.GetTempPath(), "update.msix");

                // Save the MSIX file to the temp directory
                await File.WriteAllBytesAsync(tempPath, msixData);
                Console.WriteLine("MSIX installer downloaded.");

                // Launch the MSIX installer
                LaunchMsixInstaller(tempPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading the update: " + ex.Message);
            }
        }

        // Method to launch the MSIX installer
        private void LaunchMsixInstaller(string msixPath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = msixPath,
                    Verb = "runas",  // Run with elevated privileges
                    UseShellExecute = true
                });

                Console.WriteLine("MSIX installer launched.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error launching the MSIX installer: " + ex.Message);
            }
        }
    }
}
