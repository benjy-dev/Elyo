using Elyo.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Elyo.Helpers
{
    public static class Utils
    {
        public static async Task<int> ExecuteCommandAsync(string command)
        {
            Logger.Log($"[CMD] Exécution de la commande : {command}");
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output)) Logger.Log($"[OUT] {output.Trim()}");
            if (!string.IsNullOrWhiteSpace(error)) Logger.Log($"[ERR] {error.Trim()}");

            return process.ExitCode;
        }


        public static async Task<string> CaptureCommandOutputAsync(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output)) Logger.Log($"[OUT] {output.Trim()}");
            if (!string.IsNullOrWhiteSpace(error)) Logger.Log($"[ERR] {error.Trim()}");

            return output.Trim();
        }

        public static async Task ExecuteBatchFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Log($"[ERREUR] Le script .bat '{path}' est introuvable.");
                Console.WriteLine($"[ERREUR] Le fichier '{path}' est introuvable.");
                return;
            }

            Logger.Log($"[BATCH] Exécution du script : {path}");
            await ExecuteCommandAsync($"\"{path}\"");
        }

    }
}
