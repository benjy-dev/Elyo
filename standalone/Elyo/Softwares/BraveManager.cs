using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Elyo.Helpers;
using Elyo.Services;

namespace Elyo.Softwares
{
    public class BraveManager : ISoftwareManager
    {
        private const string BraveDownloadUrl = "https://brave-browser-downloads.s3.brave.com/latest/brave_installer-x64.exe";
        private const string BraveInstallerName = "BraveSetup.exe";

        public async Task InstallAsync(string? version = null)
        {
            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine("[AVERTISSEMENT] Brave ne permet pas l'installation d'une version spécifique. La dernière version sera installée.");
                Logger.Log("[DETAILS] Brave : paramètre version ignoré parceque pas dev.");
            }

            Console.WriteLine("[INFO] Téléchargement de Brave...");
            Logger.Log("Téléchargement de Brave...");

            string tempPath = Path.Combine(Path.GetTempPath(), BraveInstallerName);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var data = await client.GetByteArrayAsync(BraveDownloadUrl);
                    await File.WriteAllBytesAsync(tempPath, data);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Erreur de téléchargement : {ex.Message}");
                    Console.WriteLine($"[ERREUR] Impossible de télécharger Brave. {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("[INFO] Installation en cours...");
            Logger.Log("Installation silencieuse de Brave...");

            int exitCode = await Utils.ExecuteCommandAsync($"\"{tempPath}\" /silent");

            if (exitCode == 0)
            {
                await Utils.ExecuteCommandAsync("taskkill /f /im brave.exe");
                await Utils.ExecuteCommandAsync("taskkill /f /im bravebrowser.exe");
                await Utils.ExecuteCommandAsync("taskkill /f /im BraveUpdate.exe");

                File.Delete(tempPath);
                Console.WriteLine("[SUCCÈS] Brave installé avec succès.");
                Logger.Log("Installation de Brave terminée.");
            }
            else
            {
                Console.WriteLine($"[ERREUR] L'installation de Brave a échoué. Code de sortie : {exitCode}");
                Logger.Log($"Installation échouée avec le code : {exitCode}");
            }
        }

        public async Task UninstallAsync()
        {
            await Utils.ExecuteCommandAsync("taskkill /f /im brave.exe");
            await Utils.ExecuteCommandAsync("taskkill /f /im bravebrowser.exe");
            await Utils.ExecuteCommandAsync("taskkill /f /im BraveUpdate.exe");

            Console.WriteLine("[INFO] Recherche du setup.exe de Brave...");
            Logger.Log("Recherche de setup.exe pour désinstallation de Brave.");


            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BraveSoftware", "Brave-Browser", "Application");

            if (!Directory.Exists(basePath))
            {
                Console.WriteLine("[ERREUR] Brave n'est pas installé ou l'installation est modifié.");
                Logger.Log("[ERREUR] Brave n'est pas installé ou l'installation est modifié.");
                Logger.Log("[DETAILS] Dossier Brave introuvable pour la désinstallation.");
                return;
            }

            var versionDirs = Directory.GetDirectories(basePath);
            foreach (var dir in versionDirs)
            {
                var setupPath = Path.Combine(dir, "Installer", "setup.exe");
                if (File.Exists(setupPath))
                {
                    string arguments = "--uninstall --force-uninstall --silent";
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

                    Console.WriteLine("[SUCCÈS] Brave désinstallé.");
                    Logger.Log("Brave désinstallé avec succès.");
                    return;
                }
            }

            Console.WriteLine("[ERREUR] setup.exe introuvable dans les dossiers de Brave.");
            Logger.Log("setup.exe non trouvé pour la désinstallation.");
        }

    }
}
