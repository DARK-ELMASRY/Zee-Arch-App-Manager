using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;

namespace ZeeAppManager
{
    internal static class PacmanHelper
    {
        internal static void RunPacmanSync()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand("sudo pacman -Sy", "Updating package database (pacman -Sy)...");
        }

        internal static void RunPacmanUpdate()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand("sudo pacman -Syu", "Updating system (pacman -Syu)...");
        }

        internal static void RunPacmanInstall(string packageList)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand($"sudo pacman -S --needed {packageList}", $"Installing: {packageList}");
        }

        internal static void RunPacmanRemove(string packageList)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand($"sudo pacman -Rns --needed {packageList}", $"Removing: {packageList}");
        }

        internal static void RunPacmanScan()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            Console.WriteLine("Scanning installed packages...");
            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", "pacman -Qq | sort");
            var packageNames = result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(IsUserFacingPackage)
                .OrderBy(p => p)
                .ToList();

            if (packageNames.Count == 0)
            {
                Console.WriteLine("No user-facing packages found. Showing full package list instead:");
                Console.WriteLine(result);
                return;
            }

            Console.WriteLine("Smart scan: app names only (no versions, no plugin packages)");
            Console.WriteLine("Type names exactly as shown below, separated by spaces, without quotes.");
            foreach (var package in packageNames)
            {
                Console.WriteLine(package);
            }
        }

        internal static void RunPacmanSearchRepo()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            Console.Write("Search repository for package: ");
            var query = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Console.WriteLine($"Searching Arch Linux package database for '{query}'...");
            var packages = SearchArchLinuxPackages(query);
            if (packages.Count == 0)
            {
                Console.WriteLine("No matching packages found in the Arch Linux database.");
                return;
            }

            Console.WriteLine("Repository package names:");
            foreach (var package in packages)
            {
                Console.WriteLine(package);
            }
            Console.WriteLine("Use these package names in install mode, without quotes.");
        }

        internal static void RunPacmanSearchInstalled()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            Console.Write("Search installed packages: ");
            var query = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Console.WriteLine($"Searching installed packages for '{query}'...");
            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", $"pacman -Qq | grep -i -- '{EscapeShellArg(query)}' | sort");
            if (string.IsNullOrWhiteSpace(result))
            {
                Console.WriteLine("No matching installed packages found.");
                return;
            }

            Console.WriteLine("Installed package names:");
            Console.WriteLine(result.Trim());
            Console.WriteLine("Use these package names in remove mode, without quotes.");
        }

        private static void ExecuteCommand(string command, string description)
        {
            Console.WriteLine(description);
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                ArgumentList = { "-c", command },
                UseShellExecute = false,
            };

            try
            {
                using var p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running command: " + ex.Message);
            }
        }

        private static List<string> SearchArchLinuxPackages(string query)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ZeeAppManager/1.0");
                var url = $"https://archlinux.org/packages/search/json/?q={Uri.EscapeDataString(query)}&page=1&n=50";
                var response = client.GetStringAsync(url).GetAwaiter().GetResult();
                using var document = JsonDocument.Parse(response);
                if (!document.RootElement.TryGetProperty("results", out var results))
                {
                    return new List<string>();
                }

                var packageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in results.EnumerateArray())
                {
                    if (item.TryGetProperty("pkgname", out var pkgNameProperty))
                    {
                        var pkgName = pkgNameProperty.GetString();
                        if (!string.IsNullOrWhiteSpace(pkgName))
                        {
                            packageNames.Add(pkgName.Trim());
                        }
                    }
                }

                return packageNames.OrderBy(p => p).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Arch Linux search failed: " + ex.Message);
                Console.WriteLine("Falling back to local pacman repo search...");
                var result = RunProcessAndCaptureOutput("/bin/bash", "-c", $"pacman -Ss '{EscapeShellArg(query)}' | awk -F' ' '/\\// {{print $1}}' | sort -u");
                return result.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
            }
        }

        private static string RunProcessAndCaptureOutput(string fileName, string arguments, string command)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                ArgumentList = { arguments, command },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            try
            {
                using var process = Process.Start(psi);
                if (process == null)
                {
                    return "Failed to start process.";
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return string.IsNullOrWhiteSpace(error) ? output : output + Environment.NewLine + error;
            }
            catch (Exception ex)
            {
                return "Error running command: " + ex.Message;
            }
        }

        private static string EscapeShellArg(string arg)
        {
            return arg.Replace("'", "'\\''");
        }

        private static bool IsUserFacingPackage(string packageName)
        {
            if (packageName.StartsWith("lib") || packageName.StartsWith("perl-") || packageName.StartsWith("python-") || packageName.StartsWith("ruby-") || packageName.StartsWith("node-") || packageName.StartsWith("rust-") || packageName.StartsWith("go-") || packageName.StartsWith("lua-"))
            {
                return false;
            }

            string[] excludeSuffixes = new[]
            {
                "-plugin", "-plugins", "-extension", "-extensions", "-addon", "-addons", "-extra",
                "-theme", "-themes", "-icon", "-icons", "-font", "-fonts",
                "-doc", "-docs", "-debug", "-bin", "-locale", "-l10n", "-lang", "-i18n", "-intl", "-man",
                "-perl", "-python", "-ruby", "-lua", "-nodejs", "-rust", "-go",
                "-gtk", "-qt", "-kde", "-gnome", "-xfce", "-lxqt", "-mate", "-cli", "-common", "-dbgsym"
            };

            foreach (var suffix in excludeSuffixes)
            {
                if (packageName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
