using System;

namespace ZeeAppManager
{
    internal static class RemoveMenu
    {
        internal static void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Remove packages:");
                Console.WriteLine("1. Scan installed packages");
                Console.WriteLine("2. Search installed package names");
                Console.WriteLine("3. Remove packages by name");
                Console.WriteLine("4. Back to main menu");
                Console.Write("Choice: ");

                if (!int.TryParse(Console.ReadLine(), out var removeChoice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    ContinuePrompt();
                    continue;
                }

                switch (removeChoice)
                {
                    case 1:
                        PacmanHelper.RunPacmanScan();
                        ContinuePrompt();
                        break;
                    case 2:
                        PacmanHelper.RunPacmanSearchInstalled();
                        ContinuePrompt();
                        break;
                    case 3:
                        RunManualRemove();
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
                return;
            }

            PacmanHelper.RunPacmanRemove(packages.Trim());
        }

        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
