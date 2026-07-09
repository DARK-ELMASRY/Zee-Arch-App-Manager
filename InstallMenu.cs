using System;
using System.Collections.Generic;
using System.Linq;

namespace ZeeAppManager
{
    // InstallMenu handles the install package submenu and related actions.
    internal static class InstallMenu
    {
        // Run displays the install submenu and loops until the user returns.
        internal static void Run()
        {
            while (true)
            {
                // Each time the submenu is shown, build the static list of install actions.
                var options = new[]
                {
                    "Most installed packages",
                    "Search package names",
                    "Enter package names manually",
                    "Back to main menu"
                };

                // Show the install menu and store the selected option index.
                var selectedIndex = ConsoleMenu.ShowMenu("Install packages:", options);
                if (selectedIndex == -1 || selectedIndex == 3)
                {
                    return;
                }

                switch (selectedIndex)
                {
                    case 0:
                        // Install from a curated list of popular packages.
                        RunInstallMostInstalled();
                        ContinuePrompt();
                        break;
                    case 1:
                        // Search the Arch Linux package repository by name.
                        RunPacmanSearchRepo();
                        ContinuePrompt();
                        break;
                    case 2:
                        // Let the user type packages manually.
                        RunManualInstall();
                        ContinuePrompt();
                        break;
                    default:
                        // Handle invalid selection values gracefully.
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        // RunManualInstall asks the user for package names and installs them.
        // Prompt the user to enter package names manually and install them.
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
                // Return without doing anything when the user requests back.
                return;
            }

            PacmanHelper.RunPacmanInstall(packages.Trim());
        }

        // Show a curated list of commonly installed apps and allow multi-select.
        private static void RunInstallMostInstalled()
        {
            string[] packages = { "firefox", "code", "discord", "steam", "spotify", "libreoffice" };
            var selectedIndexes = ConsoleMenu.ShowMultiSelect("Most installed packages:\nUse Space to select packages to install:", packages);
            if (selectedIndexes.Count == 0)
            {
                Console.WriteLine("No packages selected.");
                return;
            }

            // Build a cleaned list of unique package names from the selected indexes.
            var packageList = string.Join(' ', selectedIndexes.Select(i => packages[i]).Distinct());
            Console.WriteLine($"Installing: {packageList}");
            PacmanHelper.RunPacmanInstall(packageList);
        }

        // Delegate repository search to PacmanHelper, which handles the UI and install confirmation.
        private static void RunPacmanSearchRepo()
        {
            PacmanHelper.RunPacmanSearchRepo();
        }

        // Continue prompt pauses after a completed install action.
        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
