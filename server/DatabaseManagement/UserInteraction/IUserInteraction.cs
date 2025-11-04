namespace DatabaseManagement.UserInteraction;

/// <summary>
/// Abstracts user interaction for testing and automation
/// Allows use of the same code to run in CLI and non-interactively (tests or CI/CD)
/// </summary>
public interface IUserInteraction
{
    Task<bool> ConfirmAsync(string message);

    Task<(string username, string password)?> PromptForAdminCredentialsAsync();

    void WriteLine(string message);

    void WriteWarning(string[] messages);
    
    void WriteSuccess(string[] messages);
}