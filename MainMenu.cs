using System;
using System.Threading;

namespace ZeeAppManager
{
    // MainMenu is responsible for showing the top-level menu
    // and dispatching the user's choice to the rest of the app.
    internal static class MainMenu
    {
        // Run starts the main menu loop and waits for user input.
        internal static void Run()
        {
            // Show the title and version once before the menu appears.
            ShowStartupAnimation();

            while (true)
            {
                PrintHeader();

            // Build the top-level main menu choices shown to the user.
            var options = new[]
            {
                "Update package database (pacman -Sy)",
                "Update Arch Linux 'Full Upgrade' (pacman -Syu)",
                "Install Packages",
                "Remove Packages",
                "Exit"
            };

            // Show the menu and wait for the user to select an item.
            // The console menu returns -1 when the user presses Escape.
            var selectedIndex = ConsoleMenu.ShowMenu("Please select an option:", options);
                if (selectedIndex == -1)
                {
                    Console.WriteLine("Exiting.");
                    return;
                }

                switch (selectedIndex)
                {
                    case 0:
                        // Refresh the local pacman database only.
                        PacmanHelper.RunPacmanSync();
                        ContinuePrompt();
                        break;
                    case 1:
                        // Perform a full system upgrade with pacman.
                        PacmanHelper.RunPacmanUpdate();
                        ContinuePrompt();
                        break;
                    case 2:
                        // Enter the install submenu.
                        InstallMenu.Run();
                        break;
                    case 3:
                        // Enter the remove submenu.
                        RemoveMenu.Run();
                        break;
                    case 4:
                        // Exit the application gracefully.
                        Console.WriteLine("Exiting.");
                        return;
                    default:
                        // This case should not happen unless the menu returns an unexpected value.
                        Console.WriteLine("Invalid choice.");
                        ContinuePrompt();
                        break;
                }
            }
        }

        // ShowStartupAnimation displays the application title and version
        // on startup without any additional logo art.
        private static void ShowStartupAnimation()
        {
            // Clear the console before printing the app title.
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ZeeArch App Manager");
            Console.WriteLine("Version: 2.0.0 Snapshot w0");
            Console.ResetColor();
            // Pause briefly so the title is visible before the menu loads.
            Thread.Sleep(400);
            Thread.Sleep(400);
        }
        // PrintHeader draws the ASCII header and app metadata before each menu.
        private static void PrintHeader()
        {
            // Clear the console before drawing the header art.
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
            Console.WriteLine("Version: v1.5.0 Snapshot w0");
            Console.ResetColor();
        }

        // This method prints a numbered menu as a legacy fallback.
        // It is currently unused because the application uses the arrow-key menu helper.
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

        // ContinuePrompt pauses until the user presses Enter,
        // which gives time to read command output before the next menu.
        private static void ContinuePrompt()
        {
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
    }
}
