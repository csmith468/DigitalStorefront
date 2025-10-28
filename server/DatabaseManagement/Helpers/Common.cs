namespace DatabaseManagement.Helpers;

public static class Common
{
    public static void WriteGreenInConsole(string[] messages)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (var message in messages)
            Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }
    
    public static void WriteYellowInConsole(string[] messages)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var message in messages)
            Console.WriteLine($"\n{message}");
        Console.ResetColor();
    }
}