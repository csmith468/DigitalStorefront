namespace DatabaseManagement.UserInteraction;

public class AutoUserInteraction : IUserInteraction
{
    private readonly string _defaultUsername;
    private readonly string _defaultPassword;

    public AutoUserInteraction(string defaultUsername = "testAdmin", string defaultPassword = "TestAdmin123")
    {
        _defaultUsername = defaultUsername;
        _defaultPassword = defaultPassword;
    }

    public Task<bool> ConfirmAsync(string message)
    {
        WriteLine($"{message} [AUTO-CONFIRMED]");
        return Task.FromResult(true);
    }

    public Task<(string username, string password)?> PromptForAdminCredentialsAsync()
    {
        WriteLine($"Creating admin account: {_defaultUsername} [AUTO-GENERATED]");
        return Task.FromResult<(string, string)?>((_defaultUsername, _defaultPassword));
    }

    public void WriteLine(string message)
    {
        Console.WriteLine($"[AUTO] {message}");
    }

    public void WriteWarning(string[] messages)
    {
        Console.WriteLine("[AUTO WARNING]");
        foreach (var msg in messages)
            Console.WriteLine($"  {msg}");
    }

    public void WriteSuccess(string[] messages)
    {
        Console.WriteLine("[AUTO SUCCESS]");
        foreach (var msg in messages)
            Console.WriteLine($"  {msg}");
    }
}