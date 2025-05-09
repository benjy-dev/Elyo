using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Elyo.Helpers;
using Elyo.Services;

namespace Elyo.Softwares
{
    public class ChromeManager : ISoftwareManager
    {
        private const string ChromeDownloadUrl = "https://dl.google.com/chrome/install/latest/chrome_installer.exe";
        private const string ChromeInstallerName = "ChromeSetup.exe";

        public async Task InstallAsync(string? version = null)
        {
            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine("[AVERTISSEMENT] Google Chrome ne permet pas l'installation d'une version spécifique. La dernière version sera installée.");
                Logger.Log("[DETAILS] Chrome : paramètre version ignoré car non supporté.");
            }

            Console.WriteLine("[INFO] Téléchargement de Google Chrome...");
            Logger.Log("Téléchargement de Google Chrome...");

            string tempPath = Path.Combine(Path.GetTempPath(), ChromeInstallerName);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = await client.GetByteArrayAsync(ChromeDownloadUrl);
                    await File.WriteAllBytesAsync(tempPath, data);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Erreur de téléchargement : {ex.Message}");
                    Console.WriteLine($"[ERREUR] Impossible de télécharger Chrome. {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("[INFO] Installation en cours...");
            Logger.Log("Installation silencieuse de Google Chrome...");

            int exitCode = await Utils.ExecuteCommandAsync($"\"{tempPath}\" /silent /install");

            if (exitCode == 0)
            {
                await Utils.ExecuteCommandAsync("taskkill /f /im chrome.exe");

                File.Delete(tempPath);
                Console.WriteLine("[SUCCÈS] Google Chrome installé avec succès.");
                Logger.Log("Installation de Google Chrome terminée.");
            }
            else
            {
                Console.WriteLine($"[ERREUR] L'installation de Chrome a échoué. Code de sortie : {exitCode}");
                Logger.Log($"Installation échouée avec le code : {exitCode}");
            }
        }

        public async Task UninstallAsync()
        {
            await Utils.ExecuteCommandAsync("taskkill /f /im chrome.exe");

            Console.WriteLine("[INFO] Lancement de la désinstallation de Google Chrome...");
            Logger.Log("Tentative de désinstallation de Google Chrome...");

            string chromeUninstallPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "Application");

            if (!Directory.Exists(chromeUninstallPath))
            {
                Console.WriteLine("[ERREUR] Chrome n'est pas installé ou l'installation est modifiée.");
                Logger.Log("[ERREUR] Chrome n'est pas installé ou l'installation est modifiée.");
                Logger.Log("[DETAILS] Dossier Chrome introuvable pour la désinstallation.");
                return;
            }

            var versionDirs = Directory.GetDirectories(chromeUninstallPath);
            foreach (var dir in versionDirs)
            {
                var setupPath = Path.Combine(dir, "Installer", "setup.exe");
                if (File.Exists(setupPath))
                {
                    string arguments = "--uninstall --force-uninstall --system-level --silent";
                    Logger.Log($"[DETAILS] Exécution de : {setupPath} {arguments}");

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = setupPath,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    await process.WaitForExitAsync();

                    Console.WriteLine("[SUCCÈS] Google Chrome désinstallé.");
                    Logger.Log("Google Chrome désinstallé avec succès.");
                    return;
                }
            }

            Console.WriteLine("[ERREUR] setup.exe introuvable dans les dossiers de Chrome.");
            Logger.Log("setup.exe non trouvé pour la désinstallation.");
        }
    }
}
