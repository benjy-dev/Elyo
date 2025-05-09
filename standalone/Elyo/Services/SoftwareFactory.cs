using System;
using System.Collections.Generic;
using Elyo.Softwares;

namespace Elyo.Services
{
    public interface ISoftwareManager
    {
        Task InstallAsync(string? version = null);
        Task UninstallAsync();
    }

    public static class SoftwareFactory
    {
        private static readonly Dictionary<string, ISoftwareManager> SoftwareMap = new()
    {
        { "brave", new BraveManager() },
        { "google", new ChromeManager() } 
    };

        public static ISoftwareManager? GetSoftwareManager(string name)
        {
            return SoftwareMap.TryGetValue(name.ToLower(), out var manager) ? manager : null;
        }

        public static void ListAvailableSoftware()
        {
            Console.WriteLine("\n=== Logiciels Disponibles ===\n");
            foreach (var key in SoftwareMap.Keys)
            {
                Console.WriteLine($" - {key}");
            }
            Console.WriteLine();
        }
    }

}
