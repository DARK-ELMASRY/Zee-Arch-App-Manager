using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ZeeAppManager
{
    // SystemMenu provides monitoring and task management features.
    internal static class SystemMenu
    {
        internal static void Run()
        {
            while (true)
            {
                var options = new[]
                {
                    "Show CPU/memory usage",
                    "Show all running processes",
                    "Search running tasks",
                    "Kill task by PID",
                    "Back to main menu"
                };

                var selectedIndex = ConsoleMenu.ShowMenu("System management:", options);
                if (selectedIndex == -1 || selectedIndex == 4)
                {
                    return;
                }

                switch (selectedIndex)
                {
                    case 0:
                        ShowSystemUsage();
                        ContinuePrompt();
                        break;
                    case 1:
                        ShowAllRunningProcesses();
                        ContinuePrompt();
                        break;
                    case 2:
                        SearchRunningTasks();
                        ContinuePrompt();
                        break;
                    case 3:
                        KillTaskByPid();
                        ContinuePrompt();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        private static void ShowSystemUsage()
        {
            // Show live usage until the user presses Escape.
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Live system usage monitor (press Esc to return)");
                Console.WriteLine();

                var memorySummary = GetMemorySummary();
                if (memorySummary != null)
                {
                    Console.WriteLine($"Memory Total: {memorySummary.Value.TotalMb:F0} MB");
                    Console.WriteLine($"Memory Used:  {memorySummary.Value.UsedMb:F0} MB");
                    Console.WriteLine($"Memory Free:  {memorySummary.Value.FreeMb:F0} MB");
                    Console.WriteLine($"Memory Used:  {memorySummary.Value.UsedPercent:F1}%");
                }
                else
                {
                    Console.WriteLine("Unable to read memory usage.");
                }

                Console.WriteLine();
                Console.WriteLine("Top CPU processes:");
                Console.WriteLine();
                Console.WriteLine("PID    CPU%   MEM%   Command");

                var processes = GetTopProcesses();
                if (processes.Count == 0)
                {
                    Console.WriteLine("No process data available.");
                }
                else
                {
                    foreach (var line in processes)
                    {
                        Console.WriteLine(line);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Press Esc to return, or wait to refresh.");

                var endTime = DateTime.UtcNow + TimeSpan.FromSeconds(1);
                while (DateTime.UtcNow < endTime)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            return;
                        }
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static void ShowAllRunningProcesses()
        {
            var processes = FindProcessesByQuery(string.Empty);
            if (processes.Count == 0)
            {
                Console.WriteLine("No running processes found.");
                return;
            }

            var options = processes.Select(p => p.Command).Concat(new[] { "Back" }).ToArray();
            var selectedIndex = ConsoleMenu.ShowMenu("Running processes:", options);
            if (selectedIndex == -1 || selectedIndex == processes.Count)
            {
                return;
            }

            var selected = processes[selectedIndex];
            ShowProcessDetails(selected);
        }

        private static void SearchRunningTasks()
        {
            Console.Write("Search running tasks (leave blank to show all): ");
            var query = Console.ReadLine() ?? string.Empty;
            var processes = FindProcessesByQuery(query.Trim());
            if (processes.Count == 0)
            {
                Console.WriteLine("No running tasks matched your search.");
                return;
            }

            var options = processes.Select(p => p.Command).Concat(new[] { "Back" }).ToArray();
            var selectedIndex = ConsoleMenu.ShowMenu($"Matching tasks for '{query.Trim()}':", options);
            if (selectedIndex == -1 || selectedIndex == processes.Count)
            {
                return;
            }

            var selected = processes[selectedIndex];
            ShowProcessDetails(selected);
        }

        private static void KillTaskByPid()
        {
            Console.Write("Enter PID to kill: ");
            var pidText = Console.ReadLine();
            if (!int.TryParse(pidText, out var pid))
            {
                Console.WriteLine("Invalid PID.");
                return;
            }

            EndTask(pid.ToString());
        }

        private static List<(string Pid, string Command, string Args)> FindProcessesByQuery(string query)
        {
            var command = string.IsNullOrEmpty(query)
                ? "ps -eo pid,comm,args | tail -n +2 | sort -u"
                : $"ps -eo pid,comm,args | grep -i -- '{EscapeShellArg(query)}' | grep -v grep | sort -u";

            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", command);
            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(line =>
                {
                    var firstSpace = line.IndexOf(' ');
                    if (firstSpace < 0)
                    {
                        return (Pid: line, Command: string.Empty, Args: string.Empty);
                    }

                    var pid = line.Substring(0, firstSpace);
                    var remainder = line.Substring(firstSpace + 1).TrimStart();
                    var secondSpace = remainder.IndexOf(' ');
                    if (secondSpace < 0)
                    {
                        return (Pid: pid, Command: remainder, Args: string.Empty);
                    }

                    var commandName = remainder.Substring(0, secondSpace);
                    var args = remainder.Substring(secondSpace + 1).TrimStart();
                    return (Pid: pid, Command: commandName, Args: args);
                })
                .ToList();
        }

        private static void ShowProcessDetails((string Pid, string Command, string Args) process)
        {
            Console.Clear();
            Console.WriteLine("Task details");
            Console.WriteLine();
            Console.WriteLine($"Name: {process.Command}");
            Console.WriteLine($"PID:  {process.Pid}");
            Console.WriteLine($"Args: {process.Args}");
            Console.WriteLine();

            var location = GetProcessPath(process.Pid);
            Console.WriteLine($"Location: {location}");
            Console.WriteLine();
            Console.Write("End this task? (y/n): ");
            var confirm = Console.ReadLine();
            if (confirm?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
            {
                EndTask(process.Pid);
            }
            else
            {
                Console.WriteLine("Task not ended.");
            }
        }

        private static string GetProcessPath(string pid)
        {
            if (string.IsNullOrWhiteSpace(pid))
            {
                return "Unknown";
            }

            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", $"readlink -f /proc/{EscapeShellArg(pid)}/exe 2>/dev/null");
            return string.IsNullOrWhiteSpace(result) ? "Unknown" : result.Trim();
        }

        private static List<string> GetTopProcesses()
        {
            var result = RunProcessAndCaptureOutput("/bin/bash", "-c", "ps -eo pid,%cpu,%mem,comm --sort=-%cpu | head -n 15");
            return result.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static (double TotalMb, double UsedMb, double FreeMb, double UsedPercent)? GetMemorySummary()
        {
            var raw = RunProcessAndCaptureOutput("/bin/bash", "-c", "cat /proc/meminfo");
            var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split(':', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim();
                var valueText = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                if (double.TryParse(valueText, out var kilobytes))
                {
                    values[key] = kilobytes;
                }
            }

            if (!values.TryGetValue("MemTotal", out var totalKb) || !values.TryGetValue("MemAvailable", out var availableKb))
            {
                return null;
            }

            var totalMb = totalKb / 1024.0;
            var availableMb = availableKb / 1024.0;
            var usedMb = totalMb - availableMb;
            var usedPercent = totalMb <= 0 ? 0 : usedMb / totalMb * 100.0;
            return (totalMb, usedMb, availableMb, usedPercent);
        }

        private static void EndTask(string pid)
        {
            if (string.IsNullOrWhiteSpace(pid))
            {
                Console.WriteLine("No PID provided.");
                return;
            }

            ExecuteCommand($"sudo kill -TERM {pid}", $"Ending task {pid}...");
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
                    return string.Empty;
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return string.IsNullOrWhiteSpace(error) ? output : output + Environment.NewLine + error;
            }
            catch (Exception ex)
            {
                return $"Error running command: {ex.Message}";
            }
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
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static string EscapeShellArg(string arg)
        {
            return arg.Replace("'", "'\\''");
        }

        private static void ContinuePrompt()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
