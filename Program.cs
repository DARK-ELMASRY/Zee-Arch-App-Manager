using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

System.Console.WriteLine(@"-`                     
                 .o+`                   
                `ooo/                    
               `+oooo:                  
              `+oooooo:                  
              -+oooooo+:                 
            `/:-:++oooo+:                
           `/++++/+++++++:              
          `/++++++++++++++:              
         `/+++ooooooooooooo/`            
        ./ooosssso++osssssso+`           
       .oossssso-````/ossssss+`          
      -osssssso.      :ssssssso.         
     :osssssss/        osssso+++.        
    /ossssssss/        +ssssooo/-        
  `/ossssso+/:-        -:/+osssso+-      
 `+sso+:-`                 `.-/+oso:     
`++:.                           `-/+/    
.`                                 `/    
                                        
                                        
");


void PrintMainMenu()
{
  Console.Clear();
  System.Console.WriteLine(@"-`                     
                 .o+`                   
                `ooo/                   
               `+oooo:                  
              `+oooooo:                 
              -+oooooo+:                
            `/:-:++oooo+:               
           `/++++/+++++++:              
          `/++++++++++++++:             
         `/+++ooooooooooooo/`           
        ./ooosssso++osssssso+`          
       .oossssso-````/ossssss+`         
      -osssssso.      :ssssssso.        
     :osssssss/        osssso+++.       
    /ossssssss/        +ssssooo/-       
  `/ossssso+/:-        -:/+osssso+-     
 `+sso+:-`                 `.-/+oso:    
`++:.                           `-/+/   
.`                                 `/  
                                        ");

  Console.ForegroundColor = ConsoleColor.Cyan ;
  System.Console.WriteLine("Welcome to ZeeArch Manager!");
  System.Console.WriteLine("Author: ZeeArch");
  System.Console.WriteLine("Version: 1.0.0");
  Console.ResetColor();

  System.Console.WriteLine();
  System.Console.WriteLine("Please select an option:");
  Console.ForegroundColor = ConsoleColor.Magenta ;
  Console.WriteLine("1. Update package database (pacman -Sy)");
  Console.ForegroundColor = ConsoleColor.Yellow ;
  Console.WriteLine("2. Update Arch Linux 'Full Upgrade' (pacman -Syu)");
  Console.ForegroundColor = ConsoleColor.Green ;
  Console.WriteLine("3. Install Packages");
  Console.ForegroundColor = ConsoleColor.Gray ;
  Console.WriteLine("4. Remove Packages");
  Console.ForegroundColor = ConsoleColor.Red ;
  Console.WriteLine("5. Exit");
  Console.ResetColor();
}

int choice;
while (true)
{
  PrintMainMenu();
  Console.Write("Choice: ");
  if (!int.TryParse(Console.ReadLine(), out choice))
  {
    Console.WriteLine("Invalid input. Please enter a number.");
    continue;
  }

  switch (choice)
  {
    case 1:
      Console.WriteLine("Updating package database (pacman -Sy)...");
      RunPacmanSync();
      break;
    case 2:
      Console.WriteLine("Updating system (pacman -Syu)...");
      RunPacmanUpdate();
      break;
    case 3:
      RunInstallMenu();
      break;
    case 4:
      RunRemoveMenu();
      break;
    case 5:
      Console.WriteLine("Exiting.");
      return;
    default:
      Console.WriteLine("Invalid choice.");
      continue;
  }
}
 
static void RunPacmanSync()
{
  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
  {
    Console.WriteLine("This command only runs on Linux.");
    return;
  }

  var psi = new ProcessStartInfo
  {
    FileName = "/bin/bash",
    ArgumentList = { "-c", "sudo pacman -Sy" },
    UseShellExecute = false
  };

  try
  {
    using var p = Process.Start(psi);
    if (p != null) p.WaitForExit();
  }
  catch (Exception ex)
  {
    Console.WriteLine("Error running pacman: " + ex.Message);
  }
}

static void RunPacmanUpdate()
{
  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
  {
    Console.WriteLine("This command only runs on Linux.");
    return;
  }

  var psi = new ProcessStartInfo
  {
    FileName = "/bin/bash",
    ArgumentList = { "-c", "sudo pacman -Syu" },
    UseShellExecute = false
  };

  try
  {
    using var p = Process.Start(psi);
    if (p != null) p.WaitForExit();
  }
  catch (Exception ex)
  {
    Console.WriteLine("Error running pacman: " + ex.Message);
  }
}

static void RunInstallMenu()
{
  while (true)
  {
    Console.Clear();
    Console.WriteLine("Install packages:");
    Console.WriteLine("1. Most installed packages");
    Console.WriteLine("2. Enter package names manually");
    Console.WriteLine("3. Back to main menu");
    Console.Write("Choice: ");

    if (!int.TryParse(Console.ReadLine(), out var installChoice))
    {
      Console.WriteLine("Invalid input. Please enter a number.");
      continue;
    }

    switch (installChoice)
    {
      case 1:
        RunInstallMostInstalled();
        break;
      case 2:
        Console.WriteLine("Enter package names separated by spaces.");
        Console.WriteLine("Guide: firefox code discord steam spotify libreoffice");
        Console.WriteLine("You can type the package names directly or type 'back' to return.");
        Console.Write("Packages: ");
        var packages = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(packages))
        {
          Console.WriteLine("No packages entered.");
          continue;
        }

        if (packages.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        RunPacmanInstall(packages.Trim());
        break;
      case 3:
        return;
      default:
        Console.WriteLine("Invalid choice.");
        continue;
    }

    break;
  }
}

static void RunInstallMostInstalled()
{
  string[] packages = { "firefox", "code", "discord", "steam", "spotify", "libreoffice" };

  while (true)
  {
    Console.WriteLine();
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
      continue;
    }

    var packageList = string.Join(' ', selectedPackages.Distinct());
    Console.WriteLine($"Installing: {packageList}");
    RunPacmanInstall(packageList);
    return;
  }
}

static void RunRemoveMenu()
{
  while (true)
  {
    Console.Clear();
    Console.WriteLine("Remove packages:");
    Console.WriteLine("1. Scan installed packages");
    Console.WriteLine("2. Remove packages by name");
    Console.WriteLine("3. Back to main menu");
    Console.Write("Choice: ");

    if (!int.TryParse(Console.ReadLine(), out var removeChoice))
    {
      Console.WriteLine("Invalid input. Please enter a number.");
      continue;
    }

    switch (removeChoice)
    {
      case 1:
        RunPacmanScan();
        break;
      case 2:
        Console.WriteLine("Enter package names separated by spaces.");
        Console.WriteLine("Example: firefox code discord");
        Console.WriteLine("Type 'back' to return.");
        Console.Write("Packages: ");
        var packages = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(packages))
        {
          Console.WriteLine("No packages entered.");
          continue;
        }

        if (packages.Trim().Equals("back", StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        RunPacmanRemove(packages.Trim());
        break;
      case 3:
        return;
      default:
        Console.WriteLine("Invalid choice.");
        continue;
    }

    Console.WriteLine("Press Enter to continue.");
    Console.ReadLine();
  }
}

static void RunPacmanScan()
{
  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
  {
    Console.WriteLine("This command only runs on Linux.");
    return;
  }

  Console.WriteLine("Scanning installed packages...");
  var result = RunProcessAndCaptureOutput("/bin/bash", "-c", "pacman -Qq | sort");
  var allPackageNames = result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Select(p => p.Trim())
    .Where(p => !string.IsNullOrEmpty(p))
    .Distinct()
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

  var packageNames = allPackageNames
    .Where(IsUserFacingPackage)
    .OrderBy(p => p)
    .ToList();

  if (packageNames.Count == 0)
  {
    Console.WriteLine("No user-facing packages found. Showing full package list instead:");
    Console.WriteLine(RunProcessAndCaptureOutput("/bin/bash", "-c", "pacman -Qq | sort"));
    return;
  }

  Console.WriteLine("Smart scan: app names only (no versions, no plugin packages)");
  Console.WriteLine("Type names exactly as shown below, separated by spaces, without quotes.");
  foreach (var package in packageNames)
  {
    Console.WriteLine(package);
  }
}

static bool IsUserFacingPackage(string packageName)
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

static void RunPacmanRemove(string packageList)
{
  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
  {
    Console.WriteLine("This command only runs on Linux.");
    return;
  }

  var psi = new ProcessStartInfo
  {
    FileName = "/bin/bash",
    ArgumentList = { "-c", $"sudo pacman -Rns --needed {packageList}" },
    UseShellExecute = false
  };

  try
  {
    using var p = Process.Start(psi);
    if (p != null) p.WaitForExit();
  }
  catch (Exception ex)
  {
    Console.WriteLine("Error removing packages: " + ex.Message);
  }
}

static string RunProcessAndCaptureOutput(string fileName, string arguments, string command)
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

static void RunPacmanInstall(string packageList)
{
  if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
  {
    Console.WriteLine("This command only runs on Linux.");
    return;
  }

  var psi = new ProcessStartInfo
  {
    FileName = "/bin/bash",
    ArgumentList = { "-c", $"sudo pacman -S --needed {packageList}" },
    UseShellExecute = false
  };

  try
  {
    using var p = Process.Start(psi);
    if (p != null) p.WaitForExit();
  }
  catch (Exception ex)
  {
    Console.WriteLine("Error running pacman: " + ex.Message);
  }
}
                                        

