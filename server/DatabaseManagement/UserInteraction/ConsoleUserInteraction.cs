namespace DatabaseManagement.UserInteraction;

public class ConsoleUserInteraction : IUserInteraction
{
    public Task<bool> ConfirmAsync(string message)
    {
        Console.Write($"{message} (yes/no): ");
        var response = Console.ReadLine();
        return Task.FromResult(response?.ToLower() == "yes");
    }

    public Task<(string username, string password)?> PromptForAdminCredentialsAsync()
    {
        Console.WriteLine("Let's create your admin account...\n");

        Console.Write("Admin Username: ");
        var username = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username))
            return Task.FromResult<(string, string)?>(null);

        string? password = null;
        while (password == null)
        {
            Console.Write("Admin Password: ");
            var passwordAttempt = ReadPassword();

            Console.Write("\nConfirm Password: ");
            var confirmPassword = ReadPassword();

            if (passwordAttempt != confirmPassword)
            {
                Console.WriteLine("\nPasswords don't match! Please try again.\n");
                continue;
            }

            password = passwordAttempt;
        }

        Console.WriteLine("\n");
        return Task.FromResult<(string, string)?>((username, password));
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
    
    public void WriteSuccess(string[] messages)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (var message in messages)
            Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }
    
    public void WriteWarning(string[] messages)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var message in messages)
            Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }
    
    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
            else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        return password;
    }
}