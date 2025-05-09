using System;
using System.IO;
using System.Threading.Tasks;
using Elyo;
using Elyo.Services;
using Elyo.Helpers;

class Program
{
    private static readonly Dictionary<string, string> OptiScripts = new()
    {
        // { "brave", "Scripts\\optimize_brave.bat" }
    };

    static async Task Main(string[] args)
    {
        Logger.Init("elyo.log");

        if (args.Length == 0 || args[0] == "/?" || args[0] == "/help")
        {
            ShowHelp();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "install":
                await HandleInstall(args);
                break;

            case "uninstall":
                await HandleUninstall(args);
                break;

            case "-list":
            case "list":
                SoftwareFactory.ListAvailableSoftware();
                break;

            default:
                Console.WriteLine($"[ERREUR] Commande inconnue : {command}");
                Logger.Log($"Commande inconnue : {command}");
                ShowHelp();
                break;
        }
    }

    private static async Task HandleInstall(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("[ERREUR] Veuillez spécifier le nom du logiciel à installer.");
            return;
        }

        string softwareName = args[1].ToLower();
        string? version = null;
        bool withOpti = false;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "-v" && i + 1 < args.Length)
                version = args[i + 1];
            if (args[i] == "-opti")
                withOpti = true;
        }

        var manager = SoftwareFactory.GetSoftwareManager(softwareName);
        if (manager == null)
        {
            Console.WriteLine($"[ERREUR] Logiciel '{softwareName}' non reconnu.");
            return;
        }

        await manager.InstallAsync(version);

        if (withOpti && OptiScripts.TryGetValue(softwareName, out var scriptPath))
        {
            Console.WriteLine("[INFO] Exécution de l'optimisation personnalisée...");
            await Utils.ExecuteBatchFileAsync(scriptPath);
        }
    }

    private static async Task HandleUninstall(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("[ERREUR] Veuillez spécifier le nom du logiciel à désinstaller.");
            return;
        }

        string softwareName = args[1].ToLower();
        bool withOpti = false;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "-opti")
                withOpti = true;
        }

        var manager = SoftwareFactory.GetSoftwareManager(softwareName);
        if (manager == null)
        {
            Console.WriteLine($"[ERREUR] Logiciel '{softwareName}' non reconnu.");
            return;
        }

        await manager.UninstallAsync();

        if (withOpti && OptiScripts.TryGetValue(softwareName, out var scriptPath))
        {
            Console.WriteLine("[INFO] Exécution de l'optimisation personnalisée post-désinstallation...");
            await Utils.ExecuteBatchFileAsync(scriptPath);
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine(@"
==== Elyo Installer ====

Usage :
  elyo.exe install <logiciel> [-v <version>] [-opti]
  elyo.exe uninstall <logiciel> [-opti]
  elyo.exe -list
  elyo.exe /? or /help

Options :
  -v ""xx.xx""     Installe une version spécifique (si supporté)
  -opti           Exécute un script .bat d'optimisation si défini pour le logiciel
  -list           Affiche la liste des logiciels disponibles

Exemple :
  elyo.exe install brave 
  elyo.exe install brave -opti
  elyo.exe uninstall brave
");
    }
}
