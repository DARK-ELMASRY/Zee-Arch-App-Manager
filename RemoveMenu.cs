using System;

namespace ZeeAppManager
{
    // RemoveMenu handles the remove package submenu and package scanning.
    internal static class RemoveMenu
    {
        // Run displays the removal submenu and processes user input.
        internal static void Run()
        {
            while (true)
            {
                // Build the remove submenu options each time it is shown.
                var options = new[]
                {
                    "Scan installed packages",
                    "Search installed package names",
                    "Remove packages by name",
                    "Back to main menu"
                };

                // Show the remove menu and capture the user's decision.
                var selectedIndex = ConsoleMenu.ShowMenu("Remove packages:", options);
                if (selectedIndex == -1 || selectedIndex == 3)
                {
                    return;
                }

                switch (selectedIndex)
                {
                    case 0:
                        // Scan installed packages interactively.
                        PacmanHelper.RunPacmanScan();
                        ContinuePrompt();
                        break;
                    case 1:
                        // Search within installed package names.
                        PacmanHelper.RunPacmanSearchInstalled();
                        ContinuePrompt();
                        break;
                    case 2:
                        // Let the user remove packages by name manually.
                        RunManualRemove();
                        ContinuePrompt();
                        break;
                    default:
                        // Handle invalid selections or Escape results.
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        // Prompt the user for packages to uninstall and validate the input.
        private static void RunManualRemove()
        {
            Console.WriteLine("Enter package names separated by spaces.");
            Console.WriteLine("Example: firefox code discord");
            Console.WriteLine("Type 'back' to return.");
            Console.Write("Packages: ");
            var packages = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(packages))
            {
                Console.WriteLine("No packages entered.");
                return;
            }

            if (packages.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                // Do not attempt removal when the user cancels the prompt.
                return;
            }

            PacmanHelper.RunPacmanRemove(packages.Trim());
        }

        // ContinuePrompt waits for user input after removal actions.
        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
