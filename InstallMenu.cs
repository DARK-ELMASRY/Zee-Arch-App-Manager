using System;
using System.Collections.Generic;

namespace ZeeAppManager
{
    internal static class InstallMenu
    {
        internal static void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Install packages:");
                Console.WriteLine("1. Most installed packages");
                Console.WriteLine("2. Search package names");
                Console.WriteLine("3. Enter package names manually");
                Console.WriteLine("4. Back to main menu");
                Console.Write("Choice: ");

                if (!int.TryParse(Console.ReadLine(), out var installChoice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    ContinuePrompt();
                    continue;
                }

                switch (installChoice)
                {
                    case 1:
                        RunInstallMostInstalled();
                        ContinuePrompt();
                        break;
                    case 2:
                        RunPacmanSearchRepo();
                        ContinuePrompt();
                        break;
                    case 3:
                        RunManualInstall();
                        ContinuePrompt();
                        break;
                    case 4:
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        private static void RunManualInstall()
        {
            Console.WriteLine("Enter package names separated by spaces.");
            Console.WriteLine("Guide: firefox code discord steam spotify libreoffice");
            Console.WriteLine("You can type the package names directly or type 'back' to return.");
            Console.Write("Packages: ");
            var packages = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(packages))
            {
                Console.WriteLine("No packages entered.");
                return;
            }

            if (packages.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            PacmanHelper.RunPacmanInstall(packages.Trim());
        }

        private static void RunInstallMostInstalled()
        {
            string[] packages = { "firefox", "code", "discord", "steam", "spotify", "libreoffice" };

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Most installed packages:");
                for (int i = 0; i < packages.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {packages[i]}");
                }
                Console.WriteLine("0. Install all");
                Console.WriteLine("7. Back to install menu");
                Console.WriteLine("Tip: select numbers like 1 2 4 or type 0 to install all.");
                Console.Write("Select package numbers separated by spaces (or type back): ");

                var selection = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(selection))
                {
                    Console.WriteLine("No selection entered.");
                    ContinuePrompt();
                    continue;
                }

                var tokens = selection.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var selectedPackages = new List<string>();
                bool installAll = false;
                bool goBack = false;

                foreach (var token in tokens)
                {
                    if (token.Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        installAll = true;
                        break;
                    }
                    if (token.Equals("7", StringComparison.OrdinalIgnoreCase) || token.Equals("back", StringComparison.OrdinalIgnoreCase))
                    {
                        goBack = true;
                        break;
                    }

                    if (!int.TryParse(token, out var index) || index < 1 || index > packages.Length)
                    {
                        Console.WriteLine($"Invalid selection: {token}");
                        selectedPackages.Clear();
                        break;
                    }

                    selectedPackages.Add(packages[index - 1]);
                }

                if (goBack)
                {
                    return;
                }

                if (installAll)
                {
                    selectedPackages = packages.ToList();
                }

                if (selectedPackages.Count == 0)
                {
                    ContinuePrompt();
                    continue;
                }

                var packageList = string.Join(' ', selectedPackages.Distinct());
                Console.WriteLine($"Installing: {packageList}");
                PacmanHelper.RunPacmanInstall(packageList);
                return;
            }
        }

        private static void RunPacmanSearchRepo()
        {
            PacmanHelper.RunPacmanSearchRepo();
        }

        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
