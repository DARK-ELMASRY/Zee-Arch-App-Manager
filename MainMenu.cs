using System;

namespace ZeeAppManager
{
    internal static class MainMenu
    {
        internal static void Run()
        {
            while (true)
            {
                PrintHeader();
                PrintOptions();

                Console.Write("Choice: ");
                if (!int.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    ContinuePrompt();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        PacmanHelper.RunPacmanSync();
                        ContinuePrompt();
                        break;
                    case 2:
                        PacmanHelper.RunPacmanUpdate();
                        ContinuePrompt();
                        break;
                    case 3:
                        InstallMenu.Run();
                        break;
                    case 4:
                        RemoveMenu.Run();
                        break;
                    case 5:
                        Console.WriteLine("Exiting.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        private static void PrintHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-`                     ");
            Console.WriteLine("                 .o+`                   ");
            Console.WriteLine("                `ooo/                    ");
            Console.WriteLine("               `+oooo:                   ");
            Console.WriteLine("              `+oooooo:                  ");
            Console.WriteLine("              -+oooooo+:                 ");
            Console.WriteLine("            `/:-:++oooo+:                ");
            Console.WriteLine("           `/++++/+++++++:               ");
            Console.WriteLine("          `/++++++++++++++:              ");
            Console.WriteLine("         `/+++ooooooooooooo/`           ");
            Console.WriteLine("        ./ooosssso++osssssso+`          ");
            Console.WriteLine("       .oossssso-````/ossssss+`         ");
            Console.WriteLine("      -osssssso.      :ssssssso.        ");
            Console.WriteLine("     :osssssss/        osssso+++.       ");
            Console.WriteLine("    /ossssssss/        +ssssooo/-       ");
            Console.WriteLine("  `/ossssso+/:-        -:/+osssso+-     ");
            Console.WriteLine(" `+sso+:-`                 `.-/+oso:    ");
            Console.WriteLine("`++:.                           `-/+/   ");
            Console.WriteLine(".`                                 `/  ");
            Console.WriteLine();
            Console.WriteLine("Welcome to ZeeArch Manager!");
            Console.WriteLine("Author: ZeeArch");
            Console.WriteLine("Version: 1.0.0");
            Console.ResetColor();
        }

        private static void PrintOptions()
        {
            Console.WriteLine();
            Console.WriteLine("Please select an option:");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("1. Update package database (pacman -Sy)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("2. Update Arch Linux 'Full Upgrade' (pacman -Syu)");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("3. Install Packages");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("4. Remove Packages");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("5. Exit");
            Console.ResetColor();
        }

        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
