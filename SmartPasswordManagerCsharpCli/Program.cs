using SmartLegionLab.SmartPasswordLib;
using System.Text.Json;

namespace SmartPasswordManagerCsharpCli;

class Program
{
    private static SmartPasswordManager manager;
    private static string exportDir;

    static void Main(string[] args)
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        exportDir = Path.Combine(home, "SmartPasswordManager");
        Directory.CreateDirectory(exportDir);

        manager = new SmartPasswordManager();

        if (args.Length == 0)
        {
            InteractiveMode();
        }
        else
        {
            CommandMode(args);
        }
    }

    static void CommandMode(string[] args)
    {
        switch (args[0].ToLower())
        {
            case "add":
                if (args.Length < 4)
                {
                    Console.WriteLine("ERROR: Usage: spm add <description> <length> <secret>");
                    return;
                }
                AddPassword(args[1], args[2], args[3]);
                break;
            case "list":
                ListPasswords();
                break;
            case "get":
                if (args.Length < 2)
                {
                    Console.WriteLine("ERROR: Usage: spm get <index>");
                    return;
                }
                GetPassword(args[1]);
                break;
            case "delete":
                if (args.Length < 2)
                {
                    Console.WriteLine("ERROR: Usage: spm delete <index>");
                    return;
                }
                DeletePassword(args[1]);
                break;
            case "export":
                ExportToFile();
                break;
            case "import":
                if (args.Length < 2)
                {
                    Console.WriteLine("ERROR: Usage: spm import <filepath>");
                    return;
                }
                ImportFromFile(args[1]);
                break;
            case "help":
                ShowHelp();
                break;
            default:
                ShowHelp();
                break;
        }
    }

    static void InteractiveMode()
    {
        while (true)
        {
            Console.Clear();
            DrawHeader();
            DrawMenu();

            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddPasswordInteractive();
                    break;
                case "2":
                    GetPasswordInteractive();
                    break;
                case "3":
                    ListPasswordsInteractive();
                    break;
                case "4":
                    DeletePasswordInteractive();
                    break;
                case "5":
                    ExportInteractive();
                    break;
                case "6":
                    ImportInteractive();
                    break;
                case "7":
                    ShowHelp();
                    Console.ReadKey();
                    break;
                case "0":
                    Exit();
                    return;
                default:
                    Console.WriteLine("\nInvalid option! Press any key...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static bool IsSecretExists(string secret)
    {
        string publicKeyToCheck = SmartPasswordGenerator.GeneratePublicKey(secret);
        return manager.GetSmartPassword(publicKeyToCheck) != null;
    }

    static void AddPassword(string description, string lengthStr, string secret)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("ERROR: Description cannot be empty");
            return;
        }

        if (!int.TryParse(lengthStr, out int length) || length < 12 || length > 1000)
        {
            Console.WriteLine("ERROR: Length must be between 12 and 1000");
            return;
        }

        if (secret.Length < 12)
        {
            Console.WriteLine("ERROR: Secret must be at least 12 characters");
            return;
        }

        try
        {
            if (IsSecretExists(secret))
            {
                Console.WriteLine("ERROR: This secret phrase is already used");
                return;
            }

            string publicKey = SmartPasswordGenerator.GeneratePublicKey(secret);
            var smartPassword = new SmartPassword(publicKey, description, length);
            manager.AddSmartPassword(smartPassword);

            Console.WriteLine($"Smart password added!");
            Console.WriteLine($"Description: {description}");
            Console.WriteLine($"Length: {length}");
            Console.WriteLine($"Public Key: {publicKey}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }

    static void ListPasswords()
    {
        if (manager.PasswordCount == 0)
        {
            Console.WriteLine("No saved smart passwords found");
            return;
        }

        int index = 1;
        foreach (var sp in manager.Passwords)
        {
            Console.WriteLine($"{index}. {sp.Value.Description} (length: {sp.Value.Length})");
            index++;
        }
    }

    static void GetPassword(string indexStr)
    {
        if (!int.TryParse(indexStr, out int index) || index < 1)
        {
            Console.WriteLine("ERROR: Invalid index");
            return;
        }

        var smartPasswords = manager.Passwords.ToList();
        if (index > smartPasswords.Count)
        {
            Console.WriteLine("ERROR: Smart password not found");
            return;
        }

        var sp = smartPasswords[index - 1];
        Console.WriteLine($"Description: {sp.Value.Description}");
        Console.WriteLine($"Length: {sp.Value.Length}");
        Console.Write("Enter secret phrase: ");

        string secret = ReadSecret();

        try
        {
            if (!SmartPasswordGenerator.VerifySecret(secret, sp.Key))
            {
                Console.WriteLine("\nERROR: Invalid secret!");
                return;
            }

            string password = SmartPasswordGenerator.GenerateSmartPassword(secret, sp.Value.Length);
            Console.WriteLine($"\nPassword: {password}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
        }
    }

    static void DeletePassword(string indexStr)
    {
        if (!int.TryParse(indexStr, out int index) || index < 1)
        {
            Console.WriteLine("ERROR: Invalid index");
            return;
        }

        var smartPasswords = manager.Passwords.ToList();
        if (index > smartPasswords.Count)
        {
            Console.WriteLine("ERROR: Smart password not found");
            return;
        }

        var sp = smartPasswords[index - 1];
        if (manager.DeleteSmartPassword(sp.Key))
        {
            Console.WriteLine($"Deleted: {sp.Value.Description}");
        }
        else
        {
            Console.WriteLine("ERROR: Failed to delete");
        }
    }

    static void ExportToFile()
    {
        string fileName = $"spm_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(exportDir, fileName);

        try
        {
            var exportData = new Dictionary<string, object>();

            foreach (var sp in manager.Passwords)
            {
                var spData = new Dictionary<string, object>
                {
                    ["public_key"] = sp.Value.PublicKey,
                    ["description"] = sp.Value.Description,
                    ["length"] = sp.Value.Length
                };
                exportData[sp.Key] = spData;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(exportData, options);
            File.WriteAllText(fullPath, json);

            Console.WriteLine($"Export completed!");
            Console.WriteLine($"File: {fullPath}");
            Console.WriteLine($"Smart passwords: {manager.PasswordCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }

    static void ImportFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"ERROR: File not found: {filePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (data == null)
            {
                Console.WriteLine("ERROR: Invalid JSON format");
                return;
            }

            int imported = 0;
            int skipped = 0;

            foreach (var kv in data)
            {
                if (kv.Key.StartsWith("_"))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    string publicKey = null;
                    string description = null;
                    int length = 12;

                    if (kv.Value.TryGetProperty("public_key", out var pk))
                        publicKey = pk.GetString();

                    if (kv.Value.TryGetProperty("description", out var desc))
                        description = desc.GetString();

                    if (kv.Value.TryGetProperty("length", out var len))
                        length = len.GetInt32();

                    if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(description))
                    {
                        skipped++;
                        continue;
                    }

                    if (manager.GetSmartPassword(publicKey) != null)
                    {
                        skipped++;
                        continue;
                    }

                    var smartPassword = new SmartPassword(publicKey, description, length);
                    manager.AddSmartPassword(smartPassword);
                    imported++;
                }
                catch
                {
                    skipped++;
                }
            }

            Console.WriteLine($"Import completed!");
            Console.WriteLine($"Imported: {imported}");
            Console.WriteLine($"Skipped: {skipped}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }

    static string ReadSecret()
    {
        string secret = "";
        ConsoleKeyInfo key;

        while (true)
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace && secret.Length > 0)
            {
                secret = secret.Substring(0, secret.Length - 1);
                Console.Write("\b \b");
            }
            else if (key.KeyChar != '\b')
            {
                secret += key.KeyChar;
            }
        }

        Console.WriteLine();
        return secret;
    }

    static void ShowHelp()
    {
        Console.Clear();
        int width = Console.WindowWidth;
        Console.WriteLine(new string('=', width));
        CenterText("SMART PASSWORD MANAGER CLI");
        CenterText("Version v1.0.1");
        Console.WriteLine(new string('=', width));
        Console.WriteLine();
        Console.WriteLine("DESCRIPTION:");
        Console.WriteLine("Deterministic password manager using SmartPasswordLib");
        Console.WriteLine("Same secret + same length = same password across all platforms");
        Console.WriteLine();
        Console.WriteLine("STORAGE:");
        Console.WriteLine($"Smart passwords: {manager.FilePath}");
        Console.WriteLine($"Exports: {exportDir}");
        Console.WriteLine();
        Console.WriteLine("HOW IT WORKS:");
        Console.WriteLine("1. Add: provide secret phrase -> generates public key (stored)");
        Console.WriteLine("2. Get: select smart password -> enter secret -> verifies via public key");
        Console.WriteLine("3. Password generated from private key (never stored)");
        Console.WriteLine();
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  spm add <description> <length> <secret>");
        Console.WriteLine("  spm list");
        Console.WriteLine("  spm get <index>");
        Console.WriteLine("  spm delete <index>");
        Console.WriteLine("  spm export");
        Console.WriteLine("  spm import <filepath>");
        Console.WriteLine("  spm help");
        Console.WriteLine();
        Console.WriteLine("EXPORT/IMPORT:");
        Console.WriteLine("Export saves all smart passwords to JSON file in SmartPasswordManager folder");
        Console.WriteLine("Import reads JSON file and adds new smart passwords (skips duplicates)");
        Console.WriteLine();
        Console.WriteLine("LINKS:");
        Console.WriteLine("Repo: https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli");
        Console.WriteLine("Core Lib: https://github.com/smartlegionlab/smartpasslib-csharp");
        Console.WriteLine("License: BSD 3-Clause");
        Console.WriteLine("Author: Alexander Suvorov");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    static void DrawHeader()
    {
        int width = Console.WindowWidth;
        Console.WriteLine(new string('=', width));
        CenterText("SMART PASSWORD MANAGER CLI");
        CenterText($"Version: v1.0.1");
        CenterText($"Storage: {manager.FilePath}");
        CenterText($"Total smart passwords: {manager.PasswordCount}");
        Console.WriteLine(new string('=', width));
        Console.WriteLine();
    }

    static void DrawMenu()
    {
        Console.WriteLine(" MAIN MENU");
        Console.WriteLine(" 1. Add Smart Password");
        Console.WriteLine(" 2. Get Password");
        Console.WriteLine(" 3. List Smart Passwords");
        Console.WriteLine(" 4. Delete Smart Password");
        Console.WriteLine(" 5. Export Data");
        Console.WriteLine(" 6. Import Data");
        Console.WriteLine(" 7. Help");
        Console.WriteLine(" 0. Exit");
    }

    static void Exit()
    {
        Console.Clear();
        int width = Console.WindowWidth;
        Console.WriteLine(new string('=', width));
        CenterText("SMART PASSWORD MANAGER CLI");
        CenterText($"Version: v1.0.1");
        Console.WriteLine(new string('=', width));
        Console.WriteLine();
        CenterText("https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli");
        CenterText("Alexander Suvorov | BSD 3-Clause");
        Console.WriteLine(new string('=', width));
        Console.WriteLine();
        CenterText("Press any key to exit...");
        Console.ReadKey();
    }

    static void AddPasswordInteractive()
    {
        Console.Clear();
        DrawHeader();

        Console.WriteLine(" ADD SMART PASSWORD");
        Console.WriteLine();

        Console.Write(" Enter description: ");
        string description = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("\nERROR: Description cannot be empty!");
            Console.ReadKey();
            return;
        }

        Console.Write(" Enter password length (12-1000): ");
        if (!int.TryParse(Console.ReadLine(), out int length) || length < 12 || length > 1000)
        {
            Console.WriteLine("\nERROR: Length must be between 12 and 1000!");
            Console.ReadKey();
            return;
        }

        Console.Write(" Enter secret phrase (min 12 chars, input hidden): ");
        string secret = ReadSecret();

        if (secret.Length < 12)
        {
            Console.WriteLine("\nERROR: Secret must be at least 12 characters!");
            Console.ReadKey();
            return;
        }

        try
        {
            if (IsSecretExists(secret))
            {
                Console.WriteLine("\nERROR: This secret phrase is already used!");
                Console.ReadKey();
                return;
            }

            string publicKey = SmartPasswordGenerator.GeneratePublicKey(secret);
            var smartPassword = new SmartPassword(publicKey, description, length);
            manager.AddSmartPassword(smartPassword);

            Console.WriteLine($"\nSmart password added!");
            Console.WriteLine($"Description: {description}");
            Console.WriteLine($"Length: {length}");
            Console.WriteLine($"Public Key: {publicKey}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void GetPasswordInteractive()
    {
        Console.Clear();
        DrawHeader();

        if (manager.PasswordCount == 0)
        {
            Console.WriteLine("\nNo saved smart passwords found!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine(" SELECT SMART PASSWORD:\n");

        int index = 1;
        var smartPasswords = manager.Passwords.ToList();
        foreach (var sp in smartPasswords)
        {
            Console.WriteLine($" {index}. {sp.Value.Description} (length: {sp.Value.Length})");
            index++;
        }

        Console.Write("\nEnter number: ");
        if (!int.TryParse(Console.ReadLine(), out int selected) || selected < 1 || selected > smartPasswords.Count)
        {
            Console.WriteLine("\nERROR: Invalid selection!");
            Console.ReadKey();
            return;
        }

        var selectedSP = smartPasswords[selected - 1];
        Console.WriteLine($"\nDescription: {selectedSP.Value.Description}");
        Console.WriteLine($"Length: {selectedSP.Value.Length}");
        Console.Write("Enter secret phrase (input hidden): ");

        string secret = ReadSecret();

        try
        {
            if (!SmartPasswordGenerator.VerifySecret(secret, selectedSP.Key))
            {
                Console.WriteLine("\nERROR: Invalid secret!");
                Console.ReadKey();
                return;
            }

            string password = SmartPasswordGenerator.GenerateSmartPassword(secret, selectedSP.Value.Length);
            Console.WriteLine($"\nPassword: {password}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void ListPasswordsInteractive()
    {
        Console.Clear();
        DrawHeader();

        Console.WriteLine(" SMART PASSWORDS\n");

        if (manager.PasswordCount == 0)
        {
            Console.WriteLine(" No smart passwords found");
        }
        else
        {
            int index = 1;
            foreach (var sp in manager.Passwords)
            {
                Console.WriteLine($" {index}. {sp.Value.Description}");
                Console.WriteLine($"    Length: {sp.Value.Length}");
                Console.WriteLine($"    Public Key: {sp.Key.Substring(0, Math.Min(16, sp.Key.Length))}...");
                Console.WriteLine();
                index++;
            }
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    static void DeletePasswordInteractive()
    {
        Console.Clear();
        DrawHeader();

        if (manager.PasswordCount == 0)
        {
            Console.WriteLine("\nNo saved smart passwords found!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine(" SELECT SMART PASSWORD TO DELETE:\n");

        int index = 1;
        var smartPasswords = manager.Passwords.ToList();
        foreach (var sp in smartPasswords)
        {
            Console.WriteLine($" {index}. {sp.Value.Description} (length: {sp.Value.Length})");
            index++;
        }

        Console.Write("\nEnter number: ");
        if (!int.TryParse(Console.ReadLine(), out int selected) || selected < 1 || selected > smartPasswords.Count)
        {
            Console.WriteLine("\nERROR: Invalid selection!");
            Console.ReadKey();
            return;
        }

        var selectedSP = smartPasswords[selected - 1];
        Console.WriteLine($"\nDescription: {selectedSP.Value.Description}");
        Console.WriteLine($"Length: {selectedSP.Value.Length}");
        Console.Write("Are you sure? (y/N): ");

        if (Console.ReadLine()?.ToLower() == "y")
        {
            if (manager.DeleteSmartPassword(selectedSP.Key))
            {
                Console.WriteLine("\nSmart password deleted successfully!");
            }
            else
            {
                Console.WriteLine("\nERROR: Failed to delete!");
            }
        }
        else
        {
            Console.WriteLine("\nDeletion cancelled.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void ExportInteractive()
    {
        Console.Clear();
        DrawHeader();

        Console.WriteLine(" EXPORT DATA\n");

        if (manager.PasswordCount == 0)
        {
            Console.WriteLine(" No smart passwords to export!");
            Console.ReadKey();
            return;
        }

        ExportToFile();
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void ImportInteractive()
    {
        Console.Clear();
        DrawHeader();

        Console.WriteLine(" IMPORT DATA\n");
        Console.Write(" Enter file path: ");
        string filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("\nERROR: File path cannot be empty!");
            Console.ReadKey();
            return;
        }

        ImportFromFile(filePath);
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void CenterText(string text)
    {
        int windowWidth = Console.WindowWidth;
        int padding = (windowWidth - text.Length) / 2;

        if (padding > 0)
            Console.WriteLine(text.PadLeft(padding + text.Length));
        else
            Console.WriteLine(text);
    }
}