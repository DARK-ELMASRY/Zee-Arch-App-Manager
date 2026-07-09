using System;
using System.Collections.Generic;

namespace ZeeAppManager
{
    internal static class ConsoleMenu
    {
        // ShowMenu displays a scrollable, single-choice menu and returns the selected index.
        internal static int ShowMenu(string title, string[] options)
        {
            int selectedIndex = 0;
            int topIndex = 0;

            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine(title);
                    Console.WriteLine();
                }

                int windowHeight = Math.Max(3, Console.WindowHeight - 7);
                int visibleCount = Math.Min(windowHeight, options.Length);

                // Adjust the top index so the selected item stays visible in the window.
                if (selectedIndex < topIndex)
                {
                    topIndex = selectedIndex;
                }
                else if (selectedIndex >= topIndex + visibleCount)
                {
                    topIndex = selectedIndex - visibleCount + 1;
                }

                for (int i = topIndex; i < topIndex + visibleCount; i++)
                {
                    if (i == selectedIndex)
                    {
                        // Highlight the currently selected item.
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write("  ");
                    }

                    Console.WriteLine(options[i]);
                }

                Console.ResetColor();
                if (options.Length > visibleCount)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Showing {topIndex + 1}-{topIndex + visibleCount} of {options.Length}");
                }
                Console.WriteLine();
                Console.WriteLine("Use ↑/↓ to move, PgUp/PgDn to page, Enter to select, Esc to go back.");

                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    // Move selection up, wrapping to the bottom if needed.
                    selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    // Move selection down, wrapping to the top if needed.
                    selectedIndex = (selectedIndex + 1) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.PageUp)
                {
                    // Move one page up in the visible list.
                    selectedIndex = Math.Max(0, selectedIndex - visibleCount);
                }
                else if (keyInfo.Key == ConsoleKey.PageDown)
                {
                    // Move one page down in the visible list.
                    selectedIndex = Math.Min(options.Length - 1, selectedIndex + visibleCount);
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // Use -1 to indicate that the user canceled or went back.
                    return -1;
                }
            }
        }

        // ShowMultiSelect displays a selectable list with checkboxes.
        // The user can toggle items with Space and press Enter to finish.
        internal static List<int> ShowMultiSelect(string title, string[] options)
        {
            int selectedIndex = 0;
            var selected = new bool[options.Length];

            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine(title);
                    Console.WriteLine();
                }

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        // Highlight the currently selected row in the list.
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.Write("  ");
                    }

                    var marker = selected[i] ? "[x] " : "[ ] ";
                    Console.WriteLine(marker + options[i]);
                }

                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Use ↑/↓ to move, Space to toggle, Enter to install selected, Esc to cancel.");

                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex + 1) % options.Length;
                }
                else if (keyInfo.Key == ConsoleKey.Spacebar)
                {
                    // Toggle the checkbox for the selected line.
                    selected[selectedIndex] = !selected[selectedIndex];
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // Return the list of selected indexes when the user confirms.
                    var chosen = new List<int>();
                    for (int i = 0; i < selected.Length; i++)
                    {
                        if (selected[i]) chosen.Add(i);
                    }

                    return chosen;
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // Escape cancels the multi-select and returns an empty selection.
                    return new List<int>();
                }
            }
        }
    }
}
