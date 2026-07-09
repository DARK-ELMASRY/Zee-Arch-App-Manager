using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;

namespace ZeeAppManager
{
    // PacmanHelper contains small helper methods to call pacman,
    // search packages, and manage installed package information.
    internal static class PacmanHelper
    {
        // RunPacmanSync refreshes the pacman package database.
        internal static void RunPacmanSync()
        {
            // Only run on Linux because pacman is not available elsewhere.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand("sudo pacman -Sy", "Updating package database (pacman -Sy)...");
        }

        // RunPacmanUpdate performs a full system update using pacman.
        internal static void RunPacmanUpdate()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand("sudo pacman -Syu", "Updating system (pacman -Syu)...");
        }

        // RunPacmanInstall installs one or more packages with pacman.
        internal static void RunPacmanInstall(string packageList)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand($"sudo pacman -S --needed {packageList}", $"Installing: {packageList}");
        }

        // RunPacmanRemove removes one or more packages from the system.
        internal static void RunPacmanRemove(string packageList)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("This command only runs on Linux.");
                return;
            }

            ExecuteCommand($"sudo pacman -Rns {packageList}", $"Removing: {packageList}");
        }

        // RunPacmanScan lists installed packages and allows the user to search and select one.
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
                .OrderBy(p => p)
                .ToList();

            if (packageNames.Count == 0)
            {
                Console.WriteLine("No installed packages found.");
                return;
            }

            Console.WriteLine("Installed package scan:");
            Console.WriteLine("Use the arrow keys to move through the list, Enter to select an app.");
            Console.WriteLine();
            Console.Write("Search installed packages (leave blank to show all): ");
            var searchQuery = Console.ReadLine()?.Trim();
            var filteredPackages = string.IsNullOrWhiteSpace(searchQuery)
                ? packageNames
                : packageNames.Where(p => p.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filteredPackages.Count == 0)
            {
                Console.WriteLine("No installed packages matched your search.");
                return;
            }

            Console.WriteLine();
            var selectedIndex = ConsoleMenu.ShowMenu("Installed packages:", filteredPackages.ToArray());
            if (selectedIndex == -1)
            {
                Console.WriteLine("Returned to remove menu.");
                return;
            }

            var selectedPackage = filteredPackages[selectedIndex];
            RunPackageActionMenu(selectedPackage);
        }

        // RunPackageActionMenu shows actions for a selected installed package.
        private static void RunPackageActionMenu(string packageName)
        {
            var options = new[]
            {
                "End running task",
                "Uninstall package",
                "Back"
            };

            var selectedIndex = ConsoleMenu.ShowMenu($"Selected package: {packageName}", options);
            if (selectedIndex == -1 || selectedIndex == 2)
            {
                return;
            }

            switch (selectedIndex)
            {
                case 0:
                    // If a package is running, allow the user to terminate related processes.
                    RunPackageTaskMenu(packageName);
                    break;
                case 1:
                    // Confirm and uninstall the selected package.
                    ConfirmAndUninstallPackage(packageName);
                    break;
            }
        }

        // RunPackageTaskMenu lists running processes with names that match the package.
        private static void RunPackageTaskMenu(string packageName)
        {
            var processes = GetPackageProcesses(packageName);
            if (processes.Count == 0)
            {
                Console.WriteLine($"No running process found for '{packageName}'.");
                return;
            }

            // Show each found process, and provide a kill-all option.
            var options = processes.Select(p => $"{p.Pid}: {p.Command}").ToList();
            options.Add("Kill all matching processes");
            options.Add("Back");

            var selectedIndex = ConsoleMenu.ShowMenu($"Running processes for {packageName}:", options.ToArray());
            if (selectedIndex == -1 || selectedIndex == options.Count - 1)
            {
                return;
            }

            if (selectedIndex == options.Count - 2)
            {
                ExecuteCommand($"sudo pkill -f -- '{EscapeShellArg(packageName)}'", $"Killing all processes matching {packageName}...");
                return;
            }

            var selectedProcess = processes[selectedIndex];
            ExecuteCommand($"sudo kill -TERM {selectedProcess.Pid}", $"Killing process {selectedProcess.Pid}...");
        }

        // GetPackageProcesses returns running processes whose command line matches the package name.
        private static List<(string Pid, string Command)> GetPackageProcesses(string packageName)
        {
            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", $"pgrep -fl -- '{EscapeShellArg(packageName)}' | sort -u");
            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(line =>
                {
                    var firstSpace = line.IndexOf(' ');
                    if (firstSpace > 0)
                    {
                        return (Pid: line.Substring(0, firstSpace), Command: line.Substring(firstSpace + 1));
                    }
                    return (Pid: line, Command: string.Empty);
                })
                .ToList();
        }

        // ConfirmAndUninstallPackage asks the user before uninstalling a package.
        private static void ConfirmAndUninstallPackage(string packageName)
        {
            Console.Write($"Uninstall {packageName}? (y/n): ");
            var confirmation = Console.ReadLine();
            if (confirmation?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
            {
                RunPacmanRemove(packageName);
            }
            else
            {
                Console.WriteLine("Uninstall canceled.");
            }
        }

        // RunPacmanSearchRepo searches the Arch Linux repository and offers install selection.
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

            var menuOptions = packages.Concat(new[] { "Back" }).ToArray();
            var selectedIndex = ConsoleMenu.ShowMenu("Search results:", menuOptions);
            if (selectedIndex == -1 || selectedIndex == packages.Count)
            {
                return;
            }

            var selectedPackage = packages[selectedIndex];
            Console.WriteLine($"Selected package: {selectedPackage}");
            Console.Write($"Install {selectedPackage}? (y/n): ");
            var confirmation = Console.ReadLine();
            if (confirmation?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
            {
                RunPacmanInstall(selectedPackage);
            }
            else
            {
                Console.WriteLine("Install canceled.");
            }
        }

        // RunPacmanSearchInstalled allows the user to search through installed package names.
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

        // ExecuteCommand runs a shell command and waits for it to finish.
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

        // SearchArchLinuxPackages queries the Arch Linux website for package names.
        // If the HTTP search fails, it falls back to a local pacman repo search.
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

        // RunProcessAndCaptureOutput executes a process and returns its output and errors.
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

        // EscapeShellArg sanitizes a string for use inside a single-quoted shell argument.
        private static string EscapeShellArg(string arg)
        {
            return arg.Replace("'", "'\\''");
        }

        // IsUserFacingPackage filters out library and plugin packages from user-facing lists.
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
