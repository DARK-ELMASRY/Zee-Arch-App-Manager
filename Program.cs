using System.Diagnostics;
using System.Runtime.InteropServices;

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

int choice;
while (true)
{
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
      Console.WriteLine("Install packages - not implemented.");
      break;
    case 4:
      Console.WriteLine("Remove packages - not implemented.");
      break;
    case 5:
      Console.WriteLine("Exiting.");
      return;
    default:
      Console.WriteLine("Invalid choice.");
      continue;
  }

  break;
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
                                        

